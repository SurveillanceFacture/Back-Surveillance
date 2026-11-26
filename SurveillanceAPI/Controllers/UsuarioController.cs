
using SurveillanceAPI.Controllers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SurveillanceAPI.Applications.Service;
using SurveillanceAPI.Domains;
using SurveillanceAPI.Dto;
using SurveillanceAPI.Dto.UsuarioDto;
using SurveillanceAPI.Exceptions;
 
namespace SurveillanceAPI.Controllers;
 
[ApiController]
[Route("api/usuarios")]
[Authorize] 
public class UsuarioController : ControllerBase
{
    private const string Admin = nameof(TipoUsuarioEnum.Administrador);
 
    private readonly UsuarioService _service;
 
    public UsuarioController(UsuarioService service)
    {
        _service = service;
    }
 
    private Guid UsuarioLogadoId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new DomainException("Credenciais Invalidas");
    
 
    [HttpPost]
    [Authorize(Roles = Admin)]
    [EnableRateLimiting("admin")]
    [ProducesResponseType(typeof(LerUsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LerUsuarioDto>> Criar(CriarUsuarioDto dto, CancellationToken ct)
    {
        var criado = await _service.CriarAsync(dto, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.UsuarioId }, criado);
    }
 
    [HttpGet]
    [Authorize(Roles = Admin)]
    public async Task<ActionResult<PaginaDto<LerUsuarioDto>>> Listar(
        [FromQuery] FiltroUsuarioDto filtro, CancellationToken ct)
    {
        return Ok(await _service.ListarAsync(filtro, ct));
    }
 
    [HttpGet("{id:guid}")]
    [Authorize(Roles = Admin)]
    [ProducesResponseType(typeof(LerUsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LerUsuarioDto>> Obter(Guid id, CancellationToken ct)
    {
        return Ok(await _service.ObterAsync(id, ct));
    }
 
    /// <summary>Altera nome e tipo do usuário. O e-mail NÃO é alterado aqui: use PUT {id}/email.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = Admin)]
    [EnableRateLimiting("admin")]
    [ProducesResponseType(typeof(LerUsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LerUsuarioDto>> Atualizar(Guid id, AtualizarUsuarioDto dto, CancellationToken ct)
    {
        return Ok(await _service.AtualizarAsync(id, dto, UsuarioLogadoId, ct));
    }
 
    
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desativar(Guid id, CancellationToken ct)
    {
        await _service.DesativarAsync(id, UsuarioLogadoId, ct);
        return NoContent();
    }
 
    [HttpPost("{id:guid}/reativar")]
    [Authorize(Roles = Admin)]
    [EnableRateLimiting("admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reativar(Guid id, CancellationToken ct)
    {
        await _service.ReativarAsync(id, ct);
        return NoContent();
    }
    
    /// <summary>Único caminho para trocar o e-mail de outro usuário (admin). Registra no log quem trocou.</summary>
    [HttpPut("{id:guid}/email")]
    [Authorize(Roles = Admin)]
    [ProducesResponseType(typeof(LerUsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LerUsuarioDto>> CorrigirEmail(Guid id, CorrigirEmailDto dto, CancellationToken ct)
    {
        return Ok(await _service.CorrigirEmailAsync(id, dto, UsuarioLogadoId, ct));
    }
 
    [HttpPut("{id:guid}/senha")]
    [Authorize(Roles = Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RedefinirSenha(Guid id, RedefinirSenhaDto dto, CancellationToken ct)
    {
        await _service.RedefinirSenhaAsync(id, dto, ct);
        return NoContent();
    }
 
    // ---------- O próprio usuário ----------
 
    [HttpGet("me")]
    [ProducesResponseType(typeof(LerUsuarioDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<LerUsuarioDto>> Eu(CancellationToken ct)
    {
        return Ok(await _service.ObterAsync(UsuarioLogadoId, ct));
    }
 
    [HttpPut("me/senha")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AlterarMinhaSenha(AlterarSenhaDto dto, CancellationToken ct)
    {
        await _service.AlterarMinhaSenhaAsync(UsuarioLogadoId, dto, ct);
        return NoContent();
    }
 
    /// <summary>O próprio usuário altera o seu nome. Exige a senha atual.</summary>
    [HttpPut("me")]
    [EnableRateLimiting("perfil")]
    [ProducesResponseType(typeof(LerUsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LerUsuarioDto>> AlterarMeuNome(AlterarMeuNomeDto dto, CancellationToken ct)
    {
        return Ok(await _service.AlterarMeuNomeAsync(UsuarioLogadoId, dto, ct));
    }
 
    /// <summary>O próprio usuário altera o seu e-mail de login. Exige a senha atual.</summary>
    [HttpPut("me/email")]
    [EnableRateLimiting("perfil")]
    [ProducesResponseType(typeof(LerUsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LerUsuarioDto>> AlterarMeuEmail(AlterarMeuEmailDto dto, CancellationToken ct)
    {
        return Ok(await _service.AlterarMeuEmailAsync(UsuarioLogadoId, dto, ct));
    }
}
