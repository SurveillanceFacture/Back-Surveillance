using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using SurveillanceAPI.Interface;

namespace SurveillanceAPI.Applications.Transcricao;

public sealed class GroqChatClient : IGroqChatClient
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly HttpClient _http;
    private readonly GroqOptions _opt;
    private readonly ILogger<GroqChatClient> _log;

    public GroqChatClient(HttpClient http, IOptions<GroqOptions> options, ILogger<GroqChatClient> log)
    {
        _http = http;
        _opt = options.Value;
        _log = log;
    }

    public bool Disponivel => _opt.Habilitada && !string.IsNullOrWhiteSpace(_opt.ApiKey);

    public async Task<RespostaGroq> CompletarJsonAsync(
        string systemPrompt,
        string userPrompt,
        string nomeSchema,
        object schema,
        int maxTokens,
        CancellationToken ct)
    {
        if (!Disponivel)
            throw new InvalidOperationException("Integração Groq indisponível; verifique Disponivel antes de chamar.");

        var payload = new Dictionary<string, object?>
        {
            ["model"] = _opt.Model,
            ["temperature"] = 0,
            ["max_completion_tokens"] = maxTokens,
            ["messages"] = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            ["response_format"] = new
            {
                type = "json_schema",
                json_schema = new { name = nomeSchema, strict = true, schema }
            }
        };

        if (!string.IsNullOrWhiteSpace(_opt.ReasoningEffort))
        {
            payload["reasoning_effort"] = _opt.ReasoningEffort;
            payload["include_reasoning"] = false;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOpts), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _opt.ApiKey);

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request, ct);
        }
        catch (OperationCanceledException ex) when (!ct.IsCancellationRequested)
        {
            throw new GroqApiException("A chamada à Groq excedeu o tempo limite.", null, ex);
        }
        catch (HttpRequestException ex)
        {
            throw new GroqApiException("Não foi possível se comunicar com a Groq.", null, ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                // Não repassamos nem registramos o corpo: ele pode ecoar o prompt.
                _log.LogWarning("Groq retornou HTTP {Status}.", (int)response.StatusCode);
                throw new GroqApiException(
                    $"A Groq retornou HTTP {(int)response.StatusCode}.",
                    response.StatusCode,
                    null,
                    response.Headers.RetryAfter?.Delta);
            }

            var corpo = await response.Content.ReadAsStringAsync(ct);

            Completion? completion;
            try
            {
                completion = JsonSerializer.Deserialize<Completion>(corpo, JsonOpts);
            }
            catch (JsonException ex)
            {
                throw new GroqOutputException("A resposta da Groq não pôde ser interpretada.", ex);
            }

            var escolha = completion?.Choices?.FirstOrDefault();
            var conteudo = escolha?.Message?.Content;

            if (string.IsNullOrWhiteSpace(conteudo))
                throw new GroqOutputException("A Groq retornou uma resposta vazia.");

            if (string.Equals(escolha!.FinishReason, "length", StringComparison.OrdinalIgnoreCase))
                throw new GroqOutputException("A resposta da Groq foi truncada pelo limite de tokens.");

            _log.LogInformation(
                "Groq ok. modelo={Modelo} schema={Schema} tokensEntrada={Entrada} tokensSaida={Saida}",
                _opt.Model, nomeSchema, completion!.Usage?.PromptTokens, completion.Usage?.CompletionTokens);

            return new RespostaGroq(
                conteudo,
                escolha.FinishReason,
                completion.Usage?.PromptTokens,
                completion.Usage?.CompletionTokens);
        }
    }

    private sealed class Completion
    {
        [JsonPropertyName("choices")] public List<Choice>? Choices { get; set; }
        [JsonPropertyName("usage")] public UsageInfo? Usage { get; set; }
    }

    private sealed class Choice
    {
        [JsonPropertyName("message")] public Message? Message { get; set; }
        [JsonPropertyName("finish_reason")] public string? FinishReason { get; set; }
    }

    private sealed class Message
    {
        [JsonPropertyName("content")] public string? Content { get; set; }
    }

    private sealed class UsageInfo
    {
        [JsonPropertyName("prompt_tokens")] public int? PromptTokens { get; set; }
        [JsonPropertyName("completion_tokens")] public int? CompletionTokens { get; set; }
    }
}
