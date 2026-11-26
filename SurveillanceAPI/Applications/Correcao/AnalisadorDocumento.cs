using System.Globalization;
using System.Text.RegularExpressions;
using SurveillanceAPI.Domains;
using SurveillanceAPI.Dto.ApiUpload.ERP;
using SurveillanceAPI.Dto.ApiUpload.Xml;
using SurveillanceAPI.Dto.ApiUpload.XML;
using SurveillanceAPI.Exceptions;
using SurveillanceAPI.Services.Engine;

namespace SurveillanceAPI.Applications.Correcao;

// Reaproveita o ConciliacaoEngine (as regras continuam sendo a fonte da verdade) e acrescenta:
//  1) só compara com o(s) lançamento(s) do ERP cuja chave é a da nota;
//  2) descobre o endereço exato do item no JSON do ERP;
//  3) decide, por regra fixa, o que pode ser corrigido automaticamente (sempre XML -> ERP).
public sealed class AnalisadorDocumento
{
    private const string TipoItemNaoLancado = "ItemNaoLancadoNoErp";
    private const decimal ToleranciaTotalNota = 0.05m;

    private static readonly Regex Ncm8Digitos =
        new("^[0-9]{8}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly ConciliacaoEngine _engine;

    public AnalisadorDocumento(ConciliacaoEngine engine) => _engine = engine;

    private sealed record ItemErpIndexado(string IdLancamento, int Indice, ItemMovimentacaoErpDto Item);

    private sealed class Candidata
    {
        public required DivergenciasDto Divergencia { get; init; }
        public ErpLocalizador? Alvo { get; set; }
        public ItemNotaFiscalDto? Xml { get; set; }
        public ItemMovimentacaoErpDto? Erp { get; set; }
        public AlteracaoErp? Alteracao { get; set; }
        public string? Motivo { get; set; }
    }

    public AnaliseDocumento Analisar(NotaFiscalDto nota, MovimentacaoErpDto erp)
    {
        var chave = Digitos.Somente(nota.ChaveAcesso);

        var lancamentos = erp.Movimentacoes
            .Where(m => Digitos.Somente(m.Documento?.Chave) == chave)
            .ToList();

        if (lancamentos.Count == 0)
            throw new DomainException("Nenhum lançamento do ERP corresponde à chave de acesso da nota.");

        // id_lancamento repetido no arquivo inteiro: não dá para endereçar o item com segurança.
        var idsRepetidos = erp.Movimentacoes
            .Where(m => !string.IsNullOrWhiteSpace(m.IdLancamento))
            .GroupBy(m => m.IdLancamento, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet(StringComparer.Ordinal);

        var itensErp = new List<ItemErpIndexado>();
        foreach (var lancamento in lancamentos)
        {
            for (var i = 0; i < lancamento.Itens.Count; i++)
            {
                var item = lancamento.Itens[i];
                if (item is null) continue;
                itensErp.Add(new ItemErpIndexado(lancamento.IdLancamento, i, item));
            }
        }

        var divergencias = _engine.Conciliar(Guid.Empty, nota.Itens, itensErp.Select(x => x.Item).ToList());

        var candidatas = divergencias
            .Select(d => Preparar(d, nota, itensErp, idsRepetidos))
            .ToList();

        AplicarGuardaDoTotal(candidatas);

        var planos = candidatas
            .Where(c => c.Alvo is not null && c.Alteracao is not null)
            .GroupBy(c => c.Alvo!)
            .Select(g => MontarPlano(g.Key, g.ToList()))
            .ToList();

        var analisadas = candidatas
            .Select(c => new DivergenciaAnalisada(c.Divergencia, c.Alvo, c.Alteracao, c.Motivo))
            .ToList();

        return new AnaliseDocumento(analisadas, planos);
    }

    private static Candidata Preparar(
        DivergenciasDto d,
        NotaFiscalDto nota,
        List<ItemErpIndexado> itensErp,
        HashSet<string> idsRepetidos)
    {
        var c = new Candidata { Divergencia = d };

        if (d.TipoDivergencia == TipoItemNaoLancado)
        {
            c.Motivo = MotivosSemCorrecao.ItemAusente;
            return c;
        }

        var ean = d.Ean ?? string.Empty;
        var xmlItens = nota.Itens.Where(i => i.Ean == ean).ToList();
        var erpItens = itensErp.Where(x => x.Item.Ean == ean).ToList();

        // O motor casa por EAN e pega o primeiro. Com EAN repetido, "o primeiro" pode ser o item errado.
        if (xmlItens.Count != 1 || erpItens.Count != 1)
        {
            c.Motivo = MotivosSemCorrecao.EanRepetido;
            return c;
        }

        var alvo = erpItens[0];
        if (string.IsNullOrWhiteSpace(alvo.IdLancamento) || idsRepetidos.Contains(alvo.IdLancamento))
        {
            c.Motivo = MotivosSemCorrecao.SemLocalizador;
            return c;
        }

        var xml = xmlItens[0];
        var erp = alvo.Item;

        c.Alvo = new ErpLocalizador(alvo.IdLancamento, alvo.Indice);
        c.Xml = xml;
        c.Erp = erp;

        switch (d.CampoComparado)
        {
            case "NCM":
                if (!Ncm8Digitos.IsMatch(xml.NCM ?? string.Empty))
                    c.Motivo = MotivosSemCorrecao.NcmInvalido;
                else
                    c.Alteracao = new AlteracaoErp("ncm", "NCM", erp.NCM ?? string.Empty, xml.NCM!, false);
                break;

            case "Quantidade":
                if (xml.Quantidade <= 0)
                    c.Motivo = MotivosSemCorrecao.ValorInvalido;
                else
                    c.Alteracao = new AlteracaoErp("quantidade", "Quantidade", Num(erp.Quantidade), Num(xml.Quantidade), false);
                break;

            case "ValorUnitario":
                if (xml.ValorUnitario <= 0)
                    c.Motivo = MotivosSemCorrecao.ValorInvalido;
                else
                    c.Alteracao = new AlteracaoErp("valor_unitario", "ValorUnitario", Num(erp.ValorUnitario), Num(xml.ValorUnitario), false);
                break;

            default:
                c.Motivo = MotivosSemCorrecao.SemRegra;
                break;
        }

        return c;
    }

    // Mexer em quantidade/valor unitário obriga a mexer no valor_total. Só fazemos isso quando a própria
    // nota é consistente (quantidade × valor unitário ≈ valor total); senão o item vira correção manual.
    private static void AplicarGuardaDoTotal(List<Candidata> candidatas)
    {
        var numericasPorAlvo = candidatas
            .Where(c => c.Alvo is not null && c.Alteracao is not null && c.Xml is not null
                        && (c.Alteracao.Campo == "quantidade" || c.Alteracao.Campo == "valor_unitario"))
            .GroupBy(c => c.Alvo!);

        foreach (var grupo in numericasPorAlvo)
        {
            var xml = grupo.First().Xml!;
            var consistente = Math.Abs(xml.ValorTotal - xml.Quantidade * xml.ValorUnitario) <= ToleranciaTotalNota;
            if (consistente) continue;

            foreach (var c in grupo)
            {
                c.Alteracao = null;
                c.Motivo = MotivosSemCorrecao.TotalNaoConfere;
            }
        }
    }

    private static PlanoItem MontarPlano(ErpLocalizador alvo, List<Candidata> candidatas)
    {
        var xml = candidatas[0].Xml!;
        var erp = candidatas[0].Erp!;

        var alteracoes = candidatas
            .Select(c => c.Alteracao!)
            .OrderBy(a => Ordem(a.Campo))
            .ToList();

        var mexeNoTotal = alteracoes.Any(a => a.Campo == "quantidade" || a.Campo == "valor_unitario");
        if (mexeNoTotal && erp.ValorTotal != xml.ValorTotal)
            alteracoes.Add(new AlteracaoErp("valor_total", "ValorTotal", Num(erp.ValorTotal), Num(xml.ValorTotal), true));

        return new PlanoItem(alvo, xml.NumeroItem, xml.Ean, alteracoes);
    }

    private static int Ordem(string campo) => campo switch
    {
        "ncm" => 0,
        "quantidade" => 1,
        "valor_unitario" => 2,
        _ => 9
    };

    private static string Num(decimal valor) => valor.ToString(CultureInfo.InvariantCulture);
}
