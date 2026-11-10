using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveillanceAPI.Applications.Service;
using SurveillanceAPI.Dto;

namespace SurveillanceAPI.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AutenticacaoController:ControllerBase
{
    private readonly LoginService _service;
        
    public AutenticacaoController(LoginService service)
    {
        _service = service;
    }
    
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LerLoginDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LerLoginDto>> Login(CriarLoginDto dto, CancellationToken ct)
    {
        var resultado = await _service.LoginAsync(dto, ct);
        return Ok(resultado);
    }
    
}