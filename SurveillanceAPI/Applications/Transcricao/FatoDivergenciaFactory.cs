using System.Globalization;
using System.Text;
using SurveillanceAPI.Applications.Correcao;

namespace SurveillanceAPI.Applications.Transcricao;

public static class FatoDivergenciaFactory
{
    // Mesmo texto usado pelo ConciliacaoEngine para itens da nota que não existem no ERP.
    public const string TipoItemNaoLancado = "ItemNaoLancadoNoErp";

    public static IReadOnlyList<FatoDivergencia> Criar(AnaliseDocumento analise)
    {
        var fatos = new List<FatoDivergencia>(analise.Divergencias.Count);
        for (var i = 0; i < analise.Divergencias.Count; i++)
            fatos.Add(Criar(analise.Divergencias[i], analise, $"d{i + 1}"));
        return fatos;
    }

    public static int RankGravidade(string? gravidade) =>
        gravidade?.Trim().ToLowerInvariant() switch
        {
            "critica" or "crítica" => 4,
            "alta" => 3,
            "media" or "média" => 2,
            "baixa" => 1,
            _ => 0
        };

    private static FatoDivergencia Criar(DivergenciaAnalisada a, AnaliseDocumento analise, string referencia)
    {
        var d = a.Divergencia;
        var ean = Sanitizar(d.Ean, 20);
        var gravidade = NormalizarGravidade(d.Gravidade);
        var proposta = a.Alteracao is not null;

        if (string.Equals(d.TipoDivergencia, TipoItemNaoLancado, StringComparison.Ordinal))
        {
            return new FatoDivergencia
            {
                Ref = referencia,
                Tipo = TiposFato.ItemAusenteNoErp,
                Campo = "item",
                Gravidade = gravidade,
                NumeroItem = d.NumeroItem,
                Ean = ean,
                Sentido = "consta na nota fiscal e não foi lançado no ERP",
                SemCorrecao = a.MotivoSemCorrecao
            };
        }

        var campo = d.CampoComparado;
        string? totalAtual = null, totalProposto = null;

        if (proposta && a.Alvo is not null && campo is "Quantidade" or "ValorUnitario")
        {
            var derivada = analise.Planos
                .FirstOrDefault(p => p.Alvo == a.Alvo)?
                .Alteracoes.FirstOrDefault(x => x.Derivada);

            if (derivada is not null)
            {
                totalAtual = FormatarMoeda(derivada.Antes);
                totalProposto = FormatarMoeda(derivada.Depois);
            }
        }

        var numerico = campo is "Quantidade" or "ValorUnitario";

        if (numerico && TentarLer(d.ValorXML, out var xml) && TentarLer(d.ValorERP, out var erp))
        {
            var formato = campo == "ValorUnitario" ? FormatoPtBr.Moeda : FormatoPtBr.Quantidade;
            return new FatoDivergencia
            {
                Ref = referencia,
                Tipo = TiposFato.ValorDiferente,
                Campo = Rotulo(campo),
                Gravidade = gravidade,
                NumeroItem = d.NumeroItem,
                Ean = ean,
                ValorXml = xml.ToString(formato, FormatoPtBr.Info),
                ValorErp = erp.ToString(formato, FormatoPtBr.Info),
                Diferenca = Math.Abs(xml - erp).ToString(formato, FormatoPtBr.Info),
                Sentido = erp > xml ? "ERP maior que XML" : "ERP menor que XML",
                PropostaAlterarErp = proposta,
                TotalErpAtual = totalAtual,
                TotalErpProposto = totalProposto,
                SemCorrecao = a.MotivoSemCorrecao
            };
        }

        return new FatoDivergencia
        {
            Ref = referencia,
            Tipo = TiposFato.ValorDiferente,
            Campo = Rotulo(campo),
            Gravidade = gravidade,
            NumeroItem = d.NumeroItem,
            Ean = ean,
            ValorXml = Sanitizar(d.ValorXML, 40),
            ValorErp = Sanitizar(d.ValorERP, 40),
            PropostaAlterarErp = proposta,
            TotalErpAtual = totalAtual,
            TotalErpProposto = totalProposto,
            SemCorrecao = a.MotivoSemCorrecao
        };
    }

    private static string Rotulo(string? campo) => campo switch
    {
        "Quantidade" => "quantidade",
        "ValorUnitario" => "valor unitário",
        "NCM" => "NCM",
        _ => Sanitizar(campo, 40) ?? "campo"
    };

    private static string NormalizarGravidade(string? gravidade) =>
        gravidade?.Trim().ToLowerInvariant() switch
        {
            "critica" or "crítica" => "Crítica",
            "alta" => "Alta",
            "media" or "média" => "Média",
            "baixa" => "Baixa",
            _ => "Não informada"
        };

    private static string? FormatarMoeda(string invariante) =>
        decimal.TryParse(invariante, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out var v)
            ? v.ToString(FormatoPtBr.Moeda, FormatoPtBr.Info)
            : null;

    // As regras emitem números em InvariantCulture. Ainda assim aceita "10,5" e "1.234,50".
    private static bool TentarLer(string? texto, out decimal valor)
    {
        valor = 0;
        if (string.IsNullOrWhiteSpace(texto)) return false;
        texto = texto.Trim();

        if (decimal.TryParse(texto,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out valor))
            return true;

        return FormatoPtBr.TentarLerPtBr(texto, out valor);
    }

    // Valores vêm de XML de terceiros: só letras, dígitos e pontuação básica, tamanho curto.
    public static string? Sanitizar(string? valor, int maximo)
    {
        if (string.IsNullOrWhiteSpace(valor)) return null;

        var sb = new StringBuilder(Math.Min(valor.Length, maximo));
        foreach (var c in valor.Trim())
        {
            if (char.IsLetterOrDigit(c) || c is ' ' or '.' or ',' or '-' or '/' or '_')
                sb.Append(c);
            if (sb.Length >= maximo) break;
        }

        var resultado = sb.ToString().Trim();
        return resultado.Length == 0 ? null : resultado;
    }
}
