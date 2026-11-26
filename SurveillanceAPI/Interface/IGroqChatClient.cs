using SurveillanceAPI.Applications.Transcricao;

namespace SurveillanceAPI.Interface;

public interface IGroqChatClient
{
    // false quando a integração está desligada ou sem chave: o chamador usa o fallback.
    bool Disponivel { get; }

    Task<RespostaGroq> CompletarJsonAsync(
        string systemPrompt,
        string userPrompt,
        string nomeSchema,
        object schema,
        int maxTokens,
        CancellationToken ct);
}