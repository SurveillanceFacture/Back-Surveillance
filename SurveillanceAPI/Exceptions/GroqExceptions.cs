using System.Net;

namespace SurveillanceAPI.Applications.Groq;

public sealed class GroqConfigurationException : Exception
{
    public GroqConfigurationException()
        : base("A integração Groq não está configurada. Defina GROQ_API_KEY no ambiente da API.")
    {
    }
}

public sealed class GroqApiException : Exception
{
    public HttpStatusCode? StatusCode { get; }

    public GroqApiException(string message, HttpStatusCode? statusCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}

public sealed class GroqOutputException : Exception
{
    public GroqOutputException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}