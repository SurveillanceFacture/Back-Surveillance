using System.Net;
using SurveillanceAPI.Applications.Groq;
using SurveillanceAPI.Dto.DivergenciaDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveillanceAPI.Interface;

namespace SurveillanceAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class DivergenciaController : ControllerBase
{
    private readonly IGroqDivergenciaService _groqService;
    private readonly ILogger<DivergenciaController> _logger;

    public DivergenciaController(
        IGroqDivergenciaService groqService,
        ILogger<DivergenciaController> logger)
    {
        _groqService = groqService;
        _logger = logger;
    }

    [HttpPost("explicar")]
    [ProducesResponseType(typeof(ExplicarDivergenciasResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ExplicarDivergenciasResponseDto>> Explicar(
        [FromBody] ExplicarDivergenciasRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.Divergencias.Count > 100)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Limite de divergências excedido",
                Detail = "Envie no máximo 100 divergências por requisição.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var ids = request.Divergencias.Select(item => item.Id).ToArray();
        if (ids.Distinct(StringComparer.OrdinalIgnoreCase).Count() != ids.Length)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "IDs duplicados",
                Detail = "Cada divergência precisa ter um ID único.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            var resposta = await _groqService.ExplicarAsync(request, cancellationToken);
            return Ok(resposta);
        }
        catch (GroqConfigurationException exception)
        {
            _logger.LogError(exception, "A integração Groq não está configurada.");
            return Problem(
                title: "Integração de IA indisponível",
                detail: "A API não está configurada para acessar o provedor de IA.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (GroqOutputException exception)
        {
            _logger.LogWarning(exception, "A Groq devolveu uma saída que não passou na validação.");
            return Problem(
                title: "Resposta inválida do provedor de IA",
                detail: "Não foi possível validar a transcrição gerada. Tente novamente.",
                statusCode: StatusCodes.Status502BadGateway);
        }
        catch (GroqApiException exception)
        {
            _logger.LogWarning(exception, "Falha na comunicação com a Groq. Status: {StatusCode}", exception.StatusCode);
            var status = exception.StatusCode == HttpStatusCode.TooManyRequests
                ? StatusCodes.Status503ServiceUnavailable
                : StatusCodes.Status502BadGateway;

            return Problem(
                title: "Falha no serviço de IA",
                detail: "Não foi possível gerar as transcrições neste momento. Tente novamente mais tarde.",
                statusCode: status);
        }
    }
}
