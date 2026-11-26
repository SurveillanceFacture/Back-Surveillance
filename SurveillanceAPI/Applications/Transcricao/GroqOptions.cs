using System.ComponentModel.DataAnnotations;

namespace SurveillanceAPI.Applications.Transcricao;

public sealed class GroqOptions
{
    public const string SectionName = "Groq";

    public bool Habilitada { get; set; } = true;

    // Defina via user-secrets (Groq:ApiKey) ou variável de ambiente GROQ_API_KEY.
    // Sem chave, a API continua funcionando só com templates (geradoPorIa = false).
    public string ApiKey { get; set; } = string.Empty;

    [Required, Url]
    public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1/";

    [Required]
    public string Model { get; set; } = "openai/gpt-oss-20b";

    // Só vale para modelos gpt-oss ("low" | "medium" | "high"). Deixe vazio em outros modelos.
    public string? ReasoningEffort { get; set; } = "low";

    [Range(5, 120)] public int TimeoutSegundos { get; set; } = 30;
    [Range(1, 50)] public int TamanhoLote { get; set; } = 15;
    [Range(1, 8)] public int ParalelismoMaximo { get; set; } = 3;
    [Range(1, 4)] public int TentativasPorLote { get; set; } = 2;

    // Quantas divergências (as mais graves primeiro) passam pela IA; as demais usam template.
    [Range(1, 300)] public int MaximoItensIa { get; set; } = 60;

    // false = valores, EAN e nº do item nunca saem da API: os itens usam só template
    // e a IA recebe apenas contagens agregadas para o resumo.
    public bool EnviarValoresParaIa { get; set; } = true;
}