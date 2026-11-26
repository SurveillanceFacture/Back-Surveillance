using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SurveillanceAPI.Applications.Service;
using SurveillanceAPI.Domains;
using SurveillanceAPI.Dto.LojaDto;
using SurveillanceAPI.Dto.UsuarioDto;
using SurveillanceAPI.Exceptions;

namespace SurveillanceAPI.Controllers;

[ApiController]
[Route("api/lojas")]
[Authorize]   // qualquer usuário logado; o serviço filtra por perfil. Gestão exige Administrador
public class LojaController : ControllerBase
{
    private const string Admin = nameof(TipoUsuarioEnum.Administrador);

    private readonly LojaService _service;

    public LojaController(LojaService service) => _service = service;

    private Guid UsuarioLogadoId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new DomainException("Credenciais Invalidas");

    private string Perfil => User.FindFirstValue(ClaimTypes.Role) ?? "";

    [HttpPost]
    [Authorize(Roles = Admin)]
    [EnableRateLimiting("admin")]
    [ProducesResponseType(typeof(LerLojaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LerLojaDto>> Criar(CriarLojaDto dto, CancellationToken ct)
    {
        var criada = await _service.CriarAsync(dto, UsuarioLogadoId, ct);
        return CreatedAtAction(nameof(Obter), new { id = criada.LojaId }, criada);
    }
    
    [HttpGet("gerentes-disponiveis")]
    [Authorize(Roles = Admin)]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioResumoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UsuarioResumoDto>>> ListarGerentesDisponiveis(
        [FromQuery] Guid? lojaId,
        [FromQuery, StringLength(100)] string? busca,
        CancellationToken ct)
    {   
        return Ok(await _service.ListarGerentesDisponiveisAsync(busca, lojaId, ct));
    }


    [HttpPut("{id:guid}")]
    [Authorize(Roles = Admin)]
    [ProducesResponseType(typeof(LerLojaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LerLojaDto>> Atualizar(Guid id, AtualizarLojaDto dto, CancellationToken ct)
    {
        return Ok(await _service.AtualizarAsync(id, dto, ct));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desativar(Guid id, CancellationToken ct)
    {
        await _service.DesativarAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reativar")]
    [Authorize(Roles = Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reativar(Guid id, CancellationToken ct)
    {
        await _service.ReativarAsync(id, ct);
        return NoContent();
    }

    // ---------- Consulta (cada perfil vê o que é seu) ----------

    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<LerLojaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginaDto<LerLojaDto>>> Listar(
        [FromQuery] FiltroLojaDto filtro, CancellationToken ct)
    {
        return Ok(await _service.ListarAsync(filtro, UsuarioLogadoId, Perfil, ct));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LerLojaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LerLojaDto>> Obter(Guid id, CancellationToken ct)
    {
        return Ok(await _service.ObterAsync(id, UsuarioLogadoId, Perfil, ct));
    }
}
