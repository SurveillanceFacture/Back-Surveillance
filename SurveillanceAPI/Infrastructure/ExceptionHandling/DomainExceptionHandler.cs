using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SurveillanceAPI.Exceptions;

namespace SurveillanceAPI.Infrastructure.ExceptionHandling;

public class DomainExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken ct)
    {
        // Só trata erros de domínio; o resto continua como 500
        if (exception is not DomainException domain)
            return false;

        // DomainExceptionHandler.cs
        var (status, titulo) = domain switch
        {
            DomainException.NaoEncontradoException => (404, "Não encontrado"),
            DomainException.ConflitoException      => (409, "Conflito"),
            DomainException.NaoAutorizadoException => (401, "Não autorizado"),
            DomainException.ProibidoException      => (403, "Acesso negado"),
            _                                      => (400, "Requisição inválida")
        };

        context.Response.StatusCode = status;

        await context.Response.WriteAsJsonAsync(
            new ProblemDetails { Status = status, Title = titulo, Detail = domain.Message },
            options: null,
            contentType: "application/problem+json",
            cancellationToken: ct);

        return true;
    }
}