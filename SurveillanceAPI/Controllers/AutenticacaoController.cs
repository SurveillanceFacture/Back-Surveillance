using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SurveillanceAPI.Applications.Service;
using SurveillanceAPI.Dto;

namespace SurveillanceAPI.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AutenticacaoController:ControllerBase
{
    private readonly AutenticacaoService _service;
        
    public AutenticacaoController(AutenticacaoService service)
    {
        _service = service;
    }
    
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [ProducesResponseType(typeof(LerLoginDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LerLoginDto>> Login(CriarLoginDto dto, CancellationToken ct)
    {
        var resultado = await _service.LoginAsync(dto, ct);
        return Ok(resultado);
    }
    
    [HttpPost("esqueci-senha")]
    [AllowAnonymous]
    [EnableRateLimiting("esqueci-senha")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EsqueciSenha(EsqueciSenhaDto dto, CancellationToken ct)
    {
        await _service.SolicitarRedefinicaoAsync(dto, ct);
        return NoContent();
    }

    [HttpPost("redefinir-senha")]
    [AllowAnonymous]
    [EnableRateLimiting("redefinir-senha")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RedefinirSenha(RedefinirSenhaComTokenDto dto, CancellationToken ct)
    {
        await _service.RedefinirComTokenAsync(dto, ct);
        return NoContent();
    }

    [HttpPost("renovar")]
    [Authorize]
    public async Task<ActionResult<LerLoginDto>> Renovar(CancellationToken ct)
    {
        var ok = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id);
        if (!ok) return Unauthorized();
        return Ok(await _service.RenovarAsync(id, ct));
    }
    
}