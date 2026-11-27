using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SurveillanceAPI.Applications.Service;
using SurveillanceAPI.Domains;
using SurveillanceAPI.Dto.CorrecaoDto;
using SurveillanceAPI.Exceptions;

namespace SurveillanceAPI.Controllers;

[ApiController]
[Route("api/correcoes")]
[Authorize(Roles = nameof(TipoUsuarioEnum.Auditor))]
public class CorrecaoController : ControllerBase
{
    private readonly CorrecaoService _service;

    public CorrecaoController(CorrecaoService service) => _service = service;

    private Guid UsuarioLogadoId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new DomainException("Credenciais Invalidas");
    
    [HttpPost("propor")]
    [EnableRateLimiting("ia")]
    [RequestSizeLimit(30_000_000)]
    [ProducesResponseType(typeof(PropostaCorrecaoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PropostaCorrecaoResponseDto>> Propor(
        [FromBody] PropostaCorrecaoRequestDto request, CancellationToken ct)
    {
        return Ok(await _service.ProporAsync(UsuarioLogadoId, request, ct));
    }

    
    [HttpPost("aplicar")]
    [EnableRateLimiting("conciliacao")]
    [RequestSizeLimit(30_000_000)]
    [ProducesResponseType(typeof(AplicacaoCorrecaoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AplicacaoCorrecaoResponseDto>> Aplicar(
        [FromBody] AplicarCorrecoesRequestDto request, CancellationToken ct)
    {
        return Ok(await _service.AplicarAsync(UsuarioLogadoId, request, ct));
    }
    
    [HttpPost("confirmar")]
    [EnableRateLimiting("conciliacao")]
    [RequestSizeLimit(30_000_000)]
    [ProducesResponseType(typeof(ConfirmacaoCorrecaoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ConfirmacaoCorrecaoResponseDto>> Confirmar(
        [FromBody] ConfirmarCorrecoesRequestDto request, CancellationToken ct)
    {
        return Ok(await _service.ConfirmarAsync(UsuarioLogadoId, request, ct));
    }
}
