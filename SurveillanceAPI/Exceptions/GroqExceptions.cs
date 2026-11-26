using System.Net;

namespace SurveillanceAPI.Applications.Transcricao;

public sealed class GroqApiException : Exception
{
    public HttpStatusCode? StatusCode { get; }
    public TimeSpan? RetryAfter { get; }

    public GroqApiException(
        string message,
        HttpStatusCode? statusCode = null,
        Exception? innerException = null,
        TimeSpan? retryAfter = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        RetryAfter = retryAfter;
    }

    // Timeout/rede, 429 e 5xx valem uma nova tentativa; 400/401/403 não.
    public bool Repetivel =>
        StatusCode is null or HttpStatusCode.TooManyRequests or >= HttpStatusCode.InternalServerError;
}

public sealed class GroqOutputException : Exception
{
    public GroqOutputException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}