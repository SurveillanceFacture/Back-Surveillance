using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using SurveillanceAPI.Applications.Correcao;
using SurveillanceAPI.Interface;

namespace SurveillanceAPI.Applications.Transcricao;

// A IA só narra fatos já calculados. Tudo que ela devolve é validado; o que falhar vira template.
// A IA NÃO decide nem executa correções: ela só descreve, em linguagem simples, o que o código propôs.
public sealed class TranscritorDivergencias : ITranscritorDivergencias
{
    private const int MaximoCaracteresItem = 400;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly Dictionary<string, object> SchemaResumo = new()
    {
        ["type"] = "object",
        ["properties"] = new Dictionary<string, object>
        {
            ["resumo"] = new Dictionary<string, object> { ["type"] = "string" }
        },
        ["required"] = new[] { "resumo" },
        ["additionalProperties"] = false
    };

    private readonly IGroqChatClient _groq;
    private readonly GroqOptions _opt;
    private readonly ILogger<TranscritorDivergencias> _log;

    public TranscritorDivergencias(
        IGroqChatClient groq,
        IOptions<GroqOptions> options,
        ILogger<TranscritorDivergencias> log)
    {
        _groq = groq;
        _opt = options.Value;
        _log = log;
    }

    public async Task<ResultadoTranscricao> TranscreverAsync(AnaliseDocumento analise, CancellationToken ct = default)
    {
        var todos = FatoDivergenciaFactory.Criar(analise);
        var dadosResumo = ResumoDados.De(todos);

        if (todos.Count == 0)
            return new ResultadoTranscricao([], TranscricaoTemplate.Resumo(dadosResumo), false);

        // Só as mais graves passam pela IA; o resto sai por template (custo e latência previsíveis).
        var paraIa = todos
            .OrderByDescending(f => FatoDivergenciaFactory.RankGravidade(f.Gravidade))
            .ThenBy(f => f.NumeroItem ?? int.MaxValue)
            .Take(_opt.MaximoItensIa)
            .ToList();

        var itensTask = TranscreverItensComIaAsync(paraIa, ct);
        var resumoTask = GerarResumoAsync(dadosResumo, ct);
        await Task.WhenAll(itensTask, resumoTask);

        var textosIa = await itensTask;
        var (resumo, resumoIa) = await resumoTask;

        var itens = todos
            .Select(f => textosIa.TryGetValue(f.Ref, out var texto)
                ? new TextoItem(texto, true)
                : new TextoItem(TranscricaoTemplate.Item(f), false))
            .ToList();

        _log.LogInformation(
            "Transcrição: total={Total} porIa={PorIa} porTemplate={PorTemplate} resumoIa={ResumoIa}",
            todos.Count, textosIa.Count, todos.Count - textosIa.Count, resumoIa);

        return new ResultadoTranscricao(itens, resumo, resumoIa);
    }

    // ---------- Itens ----------

    private async Task<Dictionary<string, string>> TranscreverItensComIaAsync(
        IReadOnlyList<FatoDivergencia> fatos,
        CancellationToken ct)
    {
        var resultado = new Dictionary<string, string>(StringComparer.Ordinal);

        // Sem IA, ou com política de não enviar valores: tudo por template.
        if (!_groq.Disponivel || !_opt.EnviarValoresParaIa) return resultado;

        using var semaforo = new SemaphoreSlim(_opt.ParalelismoMaximo);

        var tarefas = fatos.Chunk(_opt.TamanhoLote).Select(async lote =>
        {
            await semaforo.WaitAsync(ct);
            try { return await TranscreverLoteAsync(lote, ct); }
            finally { semaforo.Release(); }
        }).ToList();

        foreach (var parcial in await Task.WhenAll(tarefas))
            foreach (var (chave, valor) in parcial)
                resultado[chave] = valor;

        return resultado;
    }

    private async Task<Dictionary<string, string>> TranscreverLoteAsync(
        IReadOnlyList<FatoDivergencia> lote,
        CancellationToken ct)
    {
        var validos = new Dictionary<string, string>(StringComparer.Ordinal);
        var pendentes = lote.ToList();

        for (var tentativa = 1; tentativa <= _opt.TentativasPorLote && pendentes.Count > 0; tentativa++)
        {
            try
            {
                var resposta = await _groq.CompletarJsonAsync(
                    PromptsTranscricao.Itens,
                    $"<dados>\n{JsonSerializer.Serialize(pendentes, JsonOpts)}\n</dados>",
                    "transcricao_divergencias",
                    CriarSchemaItens(pendentes),
                    maxTokens: 500 + 140 * pendentes.Count,
                    ct);

                var saida = JsonSerializer.Deserialize<SaidaItens>(resposta.Conteudo, JsonOpts);

                foreach (var item in saida?.Itens ?? [])
                {
                    var fato = pendentes.FirstOrDefault(f => f.Ref == item.Ref);
                    if (fato is null || validos.ContainsKey(fato.Ref)) continue;

                    if (ValidadorTranscricao.EhValida(item.Transcricao, fato.ValoresPermitidos(), 15, MaximoCaracteresItem))
                        validos[fato.Ref] = item.Transcricao.Trim();
                    else
                        _log.LogWarning("Transcrição rejeitada na validação ({Ref}, tentativa {Tentativa}).", fato.Ref, tentativa);
                }
            }
            catch (GroqApiException ex)
            {
                _log.LogWarning("Falha na Groq (tentativa {Tentativa}): {Mensagem}", tentativa, ex.Message);
                if (!ex.Repetivel) break;
                if (tentativa < _opt.TentativasPorLote) await EsperarAsync(ex, tentativa, ct);
            }
            catch (GroqOutputException ex)
            {
                _log.LogWarning("Saída inválida da Groq (tentativa {Tentativa}): {Mensagem}", tentativa, ex.Message);
            }
            catch (JsonException ex)
            {
                _log.LogWarning("JSON fora do esperado (tentativa {Tentativa}): {Mensagem}", tentativa, ex.Message);
            }

            // Na próxima tentativa só reenviamos o que ainda não foi validado.
            pendentes = pendentes.Where(f => !validos.ContainsKey(f.Ref)).ToList();
        }

        return validos;
    }

    private static Dictionary<string, object> CriarSchemaItens(IReadOnlyList<FatoDivergencia> lote) => new()
    {
        ["type"] = "object",
        ["properties"] = new Dictionary<string, object>
        {
            ["itens"] = new Dictionary<string, object>
            {
                ["type"] = "array",
                ["items"] = new Dictionary<string, object>
                {
                    ["type"] = "object",
                    ["properties"] = new Dictionary<string, object>
                    {
                        // enum: o modelo só consegue devolver refs que existem neste lote
                        ["ref"] = new Dictionary<string, object>
                        {
                            ["type"] = "string",
                            ["enum"] = lote.Select(f => f.Ref).ToArray()
                        },
                        ["transcricao"] = new Dictionary<string, object> { ["type"] = "string" }
                    },
                    ["required"] = new[] { "ref", "transcricao" },
                    ["additionalProperties"] = false
                }
            }
        },
        ["required"] = new[] { "itens" },
        ["additionalProperties"] = false
    };

    // ---------- Resumo ----------

    private async Task<(string Texto, bool PorIa)> GerarResumoAsync(ResumoDados dados, CancellationToken ct)
    {
        var template = TranscricaoTemplate.Resumo(dados);

        if (!_groq.Disponivel) return (template, false);

        for (var tentativa = 1; tentativa <= _opt.TentativasPorLote; tentativa++)
        {
            try
            {
                var resposta = await _groq.CompletarJsonAsync(
                    PromptsTranscricao.Resumo,
                    $"<dados>\n{JsonSerializer.Serialize(dados, JsonOpts)}\n</dados>",
                    "resumo_divergencias",
                    SchemaResumo,
                    maxTokens: 700,
                    ct);

                var saida = JsonSerializer.Deserialize<SaidaResumo>(resposta.Conteudo, JsonOpts);

                if (ValidadorTranscricao.EhValida(saida?.Resumo, dados.ValoresPermitidos(), 20, 500))
                    return (saida!.Resumo.Trim(), true);

                _log.LogWarning("Resumo rejeitado na validação (tentativa {Tentativa}).", tentativa);
            }
            catch (GroqApiException ex)
            {
                _log.LogWarning("Falha na Groq ao gerar resumo (tentativa {Tentativa}): {Mensagem}", tentativa, ex.Message);
                if (!ex.Repetivel) break;
                if (tentativa < _opt.TentativasPorLote) await EsperarAsync(ex, tentativa, ct);
            }
            catch (GroqOutputException ex)
            {
                _log.LogWarning("Saída inválida da Groq no resumo (tentativa {Tentativa}): {Mensagem}", tentativa, ex.Message);
            }
            catch (JsonException ex)
            {
                _log.LogWarning("JSON do resumo fora do esperado (tentativa {Tentativa}): {Mensagem}", tentativa, ex.Message);
            }
        }

        return (template, false);
    }

    // Respeita Retry-After (429) com teto de 5s: a requisição do auditor não deve ficar presa esperando.
    private static Task EsperarAsync(GroqApiException ex, int tentativa, CancellationToken ct)
    {
        var espera = ex.RetryAfter ?? TimeSpan.FromSeconds(Math.Pow(2, tentativa - 1));
        if (espera > TimeSpan.FromSeconds(5)) espera = TimeSpan.FromSeconds(5);
        return Task.Delay(espera, ct);
    }

    // ---------- Saídas esperadas ----------

    private sealed class SaidaItens
    {
        [JsonPropertyName("itens")] public List<SaidaItem> Itens { get; set; } = new();
    }

    private sealed class SaidaItem
    {
        [JsonPropertyName("ref")] public string Ref { get; set; } = string.Empty;
        [JsonPropertyName("transcricao")] public string Transcricao { get; set; } = string.Empty;
    }

    private sealed class SaidaResumo
    {
        [JsonPropertyName("resumo")] public string Resumo { get; set; } = string.Empty;
    }
}
