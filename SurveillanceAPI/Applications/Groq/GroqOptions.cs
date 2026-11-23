namespace SurveillanceAPI.Applications.Groq;

public class GroqOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "openai/gpt-oss-20b";

    // false por padrão para minimizar o envio de dados possivelmente sensíveis.
    public bool EnviarValoresOriginais { get; set; } = false;
}