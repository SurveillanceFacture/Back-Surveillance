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

        var (status, titulo) = domain switch
        {
            DomainException.NaoEncontradoException => (StatusCodes.Status404NotFound, "Não encontrado"),
            DomainException.ConflitoException      => (StatusCodes.Status409Conflict, "Conflito"),
            _                      => (StatusCodes.Status400BadRequest, "Requisição inválida")
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