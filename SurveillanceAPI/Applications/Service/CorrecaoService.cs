using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using SurveillanceAPI.Applications.Correcao;
using SurveillanceAPI.Applications.Transcricao;
using SurveillanceAPI.Domains;
using SurveillanceAPI.Dto.ApiUpload.ERP;
using SurveillanceAPI.Dto.ApiUpload.Xml;
using SurveillanceAPI.Dto.CorrecaoDto;
using SurveillanceAPI.Exceptions;
using SurveillanceAPI.Infraestructure.Persistence;
using SurveillanceAPI.Interface;

namespace SurveillanceAPI.Applications.Service;

// Fluxo: PROPOR (código calcula, IA só explica) -> auditor revisa o "antes → depois" -> APLICAR.
// Nada é alterado no ERP sem uma decisão explícita, e a alteração vale só para o JSON (cópia corrigida).
public class CorrecaoService
{
    private const int MaximoItensNota = 990; // limite de itens de uma NF-e
    private static readonly TimeSpan ValidadeProposta = TimeSpan.FromMinutes(30);
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly AppDbContext _db;
    private readonly AnalisadorDocumento _analisador;
    private readonly ITranscritorDivergencias _transcritor;
    private readonly ILogger<CorrecaoService> _log;

    public CorrecaoService(
        AppDbContext db,
        AnalisadorDocumento analisador,
        ITranscritorDivergencias transcritor,
        ILogger<CorrecaoService> log)
    {
        _db = db;
        _analisador = analisador;
        _transcritor = transcritor;
        _log = log;
    }

    // ------------------------------------------------------------------ PROPOR

    public async Task<PropostaCorrecaoResponseDto> ProporAsync(
        Guid usuarioId, PropostaCorrecaoRequestDto req, CancellationToken ct)
    {
        var nota = req.NotaFiscal ?? throw new DomainException("Envie a nota fiscal.");
        var erpNode = req.Erp ?? throw new DomainException("Envie o JSON do ERP.");

        ValidarNota(nota);
        var loja = await ObterLojaDoAuditorAsync(usuarioId, nota, ct);

        var erp = LerErp(erpNode);
        ValidarLojaDoErp(erp, loja);

        var analise = _analisador.Analisar(nota, erp);
        var transcricao = await _transcritor.TranscreverAsync(analise, ct);

        var agora = DateTime.UtcNow;
        var hashErp = HashJson(erpNode);

        var comparacao = new Comparacao(loja.LojaId, usuarioId, ChaveDfe.Normalizar(nota.ChaveAcesso), HashNota(nota));

        foreach (var a in analise.Divergencias)
        {
            var d = a.Divergencia;
            comparacao.AdicionarDivergencia(
                d.NumeroItem ?? 0, d.CodigoProduto, d.Ean, d.CampoComparado,
                d.ValorXML, d.ValorERP, GravidadeParser.Converter(d.Gravidade));
        }

        _db.Comparacoes.Add(comparacao);

        var propostasPorAlvo = new Dictionary<ErpLocalizador, CorrecaoErp>();
        foreach (var plano in analise.Planos)
        {
            var indices = Enumerable.Range(0, analise.Divergencias.Count)
                .Where(i => analise.Divergencias[i].Alteracao is not null
                            && analise.Divergencias[i].Alvo == plano.Alvo)
                .ToList();

            var explicacao = string.Join(" ", indices.Select(i => transcricao.Itens[i].Texto));
            var porIa = indices.Count > 0 && indices.All(i => transcricao.Itens[i].PorIa);

            var proposta = new CorrecaoErp(
                comparacao, loja.LojaId, usuarioId,
                plano.Alvo.IdLancamento, plano.Alvo.IndiceItem, plano.NumeroItem, plano.Ean,
                plano.Alteracoes, hashErp, explicacao, porIa, agora, ValidadeProposta);

            _db.CorrecoesErp.Add(proposta);
            propostasPorAlvo[plano.Alvo] = proposta;
        }

        await _db.SaveChangesAsync(ct);

        _log.LogInformation(
            "Propostas geradas: comparacao={Comparacao} divergencias={Divergencias} propostas={Propostas}",
            comparacao.ComparacaoId, analise.Divergencias.Count, propostasPorAlvo.Count);

        return MontarResposta(comparacao.ComparacaoId, analise, transcricao, propostasPorAlvo);
    }

    private static PropostaCorrecaoResponseDto MontarResposta(
        long comparacaoId,
        AnaliseDocumento analise,
        ResultadoTranscricao transcricao,
        Dictionary<ErpLocalizador, CorrecaoErp> propostasPorAlvo)
    {
        var itens = new List<ItemRevisaoDto>(analise.Divergencias.Count);

        for (var i = 0; i < analise.Divergencias.Count; i++)
        {
            var a = analise.Divergencias[i];
            var d = a.Divergencia;

            Guid? propostaId = null;
            if (a.Alteracao is not null && a.Alvo is not null
                && propostasPorAlvo.TryGetValue(a.Alvo, out var proposta))
                propostaId = proposta.CorrecaoErpId;

            itens.Add(new ItemRevisaoDto
            {
                NumeroItem = d.NumeroItem,
                Ean = d.Ean,
                Campo = d.CampoComparado,
                Gravidade = d.Gravidade,
                ValorXml = d.ValorXML,
                ValorErp = d.ValorERP,
                Transcricao = transcricao.Itens[i].Texto,
                GeradoPorIa = transcricao.Itens[i].PorIa,
                CorrecaoAutomatica = propostaId.HasValue,
                MotivoSemCorrecao = a.MotivoSemCorrecao,
                PropostaId = propostaId
            });
        }

        var propostas = propostasPorAlvo.Values
            .OrderBy(p => p.NumeroItem)
            .Select(p => new PropostaItemDto
            {
                PropostaId = p.CorrecaoErpId,
                NumeroItem = p.NumeroItem,
                Ean = p.Ean,
                IdLancamento = p.IdLancamento,
                IndiceItem = p.IndiceItem,
                Alteracoes = p.Alteracoes()
                    .Select(x => new AlteracaoDto
                    {
                        Campo = x.Campo,
                        Antes = x.Antes,
                        Depois = x.Depois,
                        Derivada = x.Derivada
                    })
                    .ToList(),
                Explicacao = p.Explicacao,
                ExplicacaoGeradaPorIa = p.ExplicacaoGeradaPorIa,
                ExpiraEm = p.ExpiraEm
            })
            .ToList();

        return new PropostaCorrecaoResponseDto
        {
            ComparacaoId = comparacaoId,
            TotalDivergencias = analise.Divergencias.Count,
            TotalCorrigiveis = itens.Count(x => x.CorrecaoAutomatica),
            Resumo = transcricao.Resumo,
            ResumoGeradoPorIa = transcricao.ResumoPorIa,
            Itens = itens,
            Propostas = propostas
        };
    }

    // ------------------------------------------------------------------ APLICAR

    public async Task<AplicacaoCorrecaoResponseDto> AplicarAsync(
        Guid usuarioId, AplicarCorrecoesRequestDto req, CancellationToken ct)
    {
        var nota = req.NotaFiscal ?? throw new DomainException("Envie a nota fiscal.");
        var erpNode = req.Erp ?? throw new DomainException("Envie o JSON do ERP.");
        ValidarNota(nota);

        var ids = req.PropostaIds.Distinct().ToList();
        if (ids.Count == 0) throw new DomainException("Selecione ao menos uma proposta.");

        var propostas = await _db.CorrecoesErp
            .Include(c => c.Comparacao)
            .Where(c => ids.Contains(c.CorrecaoErpId))
            .ToListAsync(ct);

        // Mesma resposta para "não existe" e "é de outro usuário": não revela a existência de propostas alheias.
        if (propostas.Count != ids.Count || propostas.Any(p => p.UsuarioId != usuarioId))
            throw new DomainException.NaoEncontradoException("Proposta não encontrada.");

        var agora = DateTime.UtcNow;
        foreach (var p in propostas) p.ValidarAplicavel(agora);

        var comparacao = propostas[0].Comparacao;
        if (propostas.Any(p => p.ComparacaoId != comparacao.ComparacaoId))
            throw new DomainException("As propostas precisam ser da mesma análise.");

        var loja = await ObterLojaDoAuditorAsync(usuarioId, nota, ct);
        if (loja.LojaId != comparacao.LojaId)
            throw new DomainException.ProibidoException("A nota enviada não pertence à loja da análise.");

        if (HashNota(nota) != comparacao.HashXml)
            throw new DomainException.ConflitoException("A nota enviada é diferente da que foi analisada. Gere uma nova proposta.");

        var hashErpEnviado = HashJson(erpNode);
        if (propostas.Any(p => p.HashErp != hashErpEnviado))
            throw new DomainException.ConflitoException("O JSON do ERP enviado é diferente do que foi analisado. Gere uma nova proposta.");

        var erpOriginal = LerErp(erpNode);
        ValidarLojaDoErp(erpOriginal, loja);
        var antes = _analisador.Analisar(nota, erpOriginal);

        // Escreve numa CÓPIA; o original do cliente nunca é tocado.
        var copia = erpNode.DeepClone();
        ErpJsonPatcher.Aplicar(copia, propostas);

        // Conferência: o motor roda de novo no JSON corrigido.
        var depois = _analisador.Analisar(nota, LerErp(copia));

        var chavesAntes = antes.Divergencias.Select(ChaveDe).ToHashSet();
        var chavesDepois = depois.Divergencias.Select(ChaveDe).ToHashSet();

        if (chavesDepois.Except(chavesAntes).Any())
            throw new DomainException("A conferência após a correção encontrou divergências novas. Nada foi aplicado.");

        var corrigidas = propostas
            .SelectMany(p => p.Alteracoes()
                .Where(a => !a.Derivada)
                .Select(a => (Ean: p.Ean ?? string.Empty, Campo: a.CampoComparado)))
            .ToList();

        if (corrigidas.Any(c => chavesDepois.Contains(c)))
            throw new DomainException("A conferência após a correção ainda encontrou divergências nos itens corrigidos. Nada foi aplicado.");

        var hashDepois = HashJson(copia);
        var divergenciasDb = await _db.Divergencias
            .Where(d => d.ComparacaoId == comparacao.ComparacaoId)
            .ToListAsync(ct);

        var resolvidas = 0;
        foreach (var p in propostas)
        {
            foreach (var alteracao in p.Alteracoes().Where(a => !a.Derivada))
            {
                foreach (var d in divergenciasDb)
                {
                    var aberta = d.Status == StatusDivergencia.Aberta || d.Status == StatusDivergencia.EmAnalise;
                    if (!aberta) continue;
                    if (d.CampoComparado != alteracao.CampoComparado) continue;
                    if ((d.Ean ?? string.Empty) != (p.Ean ?? string.Empty)) continue;

                    d.Resolver(usuarioId, $"Corrigido no JSON do ERP (correção {p.CorrecaoErpId}).");
                    resolvidas++;
                }
            }

            p.MarcarAplicada(usuarioId, hashDepois, agora);
        }

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new DomainException.ConflitoException("A proposta já foi aplicada ou alterada por outra requisição.");
        }

        _log.LogInformation(
            "Correções aplicadas: usuario={Usuario} comparacao={Comparacao} propostas={Propostas} resolvidas={Resolvidas}",
            usuarioId, comparacao.ComparacaoId, propostas.Count, resolvidas);

        return new AplicacaoCorrecaoResponseDto
        {
            CorrecoesAplicadas = propostas.Count,
            DivergenciasResolvidas = resolvidas,
            HashErpDepois = hashDepois,
            Restantes = depois.Divergencias
                .Select(a => new DivergenciaRestanteDto
                {
                    NumeroItem = a.Divergencia.NumeroItem,
                    Ean = a.Divergencia.Ean,
                    Campo = a.Divergencia.CampoComparado,
                    Gravidade = a.Divergencia.Gravidade
                })
                .ToList(),
            ErpCorrigido = copia
        };
    }

    // ------------------------------------------------------------------ apoio

    private static (string Ean, string Campo) ChaveDe(DivergenciaAnalisada a) =>
        (a.Divergencia.Ean ?? string.Empty, a.Divergencia.CampoComparado);

    private static void ValidarNota(NotaFiscalDto nota)
    {
        if (!ChaveDfe.Valida(ChaveDfe.Normalizar(nota.ChaveAcesso)))
            throw new DomainException("A chave de acesso da nota é inválida (44 posições; o trecho do CNPJ pode ter letras).");

        // Loja.NormalizarCnpj já aceita letras e dígitos (CNPJ alfanumérico).
        if (Loja.NormalizarCnpj(nota.CnpjDestinatario).Length != 14)
            throw new DomainException("O CNPJ do destinatário da nota é inválido.");

        if (nota.Itens.Count == 0)
            throw new DomainException("A nota não tem itens.");

        if (nota.Itens.Count > MaximoItensNota)
            throw new DomainException($"A nota excede o limite de {MaximoItensNota} itens.");
    }

    // O auditor só mexe em notas da(s) loja(s) pela(s) qual(is) é responsável.
    private async Task<Loja> ObterLojaDoAuditorAsync(Guid usuarioId, NotaFiscalDto nota, CancellationToken ct)
    {
        var cnpj = Loja.NormalizarCnpj(nota.CnpjDestinatario);

        return await _db.Lojas.FirstOrDefaultAsync(
                   l => l.Cnpj == cnpj && l.AuditorUsuarioId == usuarioId && l.StatusLoja, ct)
               ?? throw new DomainException.ProibidoException(
                   "Você não é o auditor de uma loja ativa com o CNPJ do destinatário desta nota.");
    }

    private static void ValidarLojaDoErp(MovimentacaoErpDto erp, Loja loja)
    {
        if (Loja.NormalizarCnpj(erp.Loja?.Cnpj ?? string.Empty) != loja.Cnpj)
            throw new DomainException("O CNPJ da loja no JSON do ERP não confere com a loja da nota.");
    }

    private static MovimentacaoErpDto LerErp(JsonNode node)
    {
        try
        {
            return node.Deserialize<MovimentacaoErpDto>(JsonOpts)
                   ?? throw new DomainException("O JSON do ERP está vazio.");
        }
        catch (JsonException ex)
        {
            throw new DomainException($"O JSON do ERP está fora do formato esperado (em {ex.Path}).");
        }
    }

    // Hash do JSON compacto: não muda com espaços/identação, muda com qualquer valor ou ordem de campos.
    private static string HashJson(JsonNode node) => Sha256Hex(node.ToJsonString());

    private static string HashNota(NotaFiscalDto nota) => Sha256Hex(JsonSerializer.Serialize(nota, JsonOpts));

    private static string Sha256Hex(string texto) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(texto))).ToLowerInvariant();
}
