using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveillanceAPI.Applications.Service;
using SurveillanceAPI.Domains;
using SurveillanceAPI.Dto;
using SurveillanceAPI.Dto.UsuarioDto;
using SurveillanceAPI.Exceptions;

namespace SurveillanceAPI.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize] // qualquer usuário logado; as ações de gestão exigem Administrador (abaixo)
public class UsuarioController : ControllerBase
{
    private const string SoAdmin = nameof(TipoUsuarioEnum.Administrador);

    private readonly UsuarioService _service;

    public UsuarioController(UsuarioService service)
    {
        _service = service;
    }

    // Id do usuário que está fazendo a requisição, vindo do token
    private Guid UsuarioLogadoId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new DomainException("Credenciais Invalidas");

    // ---------- Gestão (somente Administrador) ----------

    [HttpPost]
    [Authorize(Roles = SoAdmin)]
    [ProducesResponseType(typeof(LerUsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LerUsuarioDto>> Criar(CriarUsuarioDto dto, CancellationToken ct)
    {
        var criado = await _service.CriarAsync(dto, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.UsuarioId }, criado);
    }

    [HttpGet]
    [Authorize(Roles = SoAdmin)]
    public async Task<ActionResult<PaginaDto<LerUsuarioDto>>> Listar(
        [FromQuery] FiltroUsuarioDto filtro, CancellationToken ct)
    {
        return Ok(await _service.ListarAsync(filtro, ct));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = SoAdmin)]
    [ProducesResponseType(typeof(LerUsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LerUsuarioDto>> Obter(Guid id, CancellationToken ct)
    {
        return Ok(await _service.ObterAsync(id, ct));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = SoAdmin)]
    [ProducesResponseType(typeof(LerUsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LerUsuarioDto>> Atualizar(Guid id, AtualizarUsuarioDto dto, CancellationToken ct)
    {
        return Ok(await _service.AtualizarAsync(id, dto, UsuarioLogadoId, ct));
    }

    /// <summary>Desativa o usuário (não apaga: ele tem histórico no sistema).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = SoAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desativar(Guid id, CancellationToken ct)
    {
        await _service.DesativarAsync(id, UsuarioLogadoId, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reativar")]
    [Authorize(Roles = SoAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reativar(Guid id, CancellationToken ct)
    {
        await _service.ReativarAsync(id, ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/senha")]
    [Authorize(Roles = SoAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RedefinirSenha(Guid id, RedefinirSenhaDto dto, CancellationToken ct)
    {
        await _service.RedefinirSenhaAsync(id, dto, ct);
        return NoContent();
    }

    // ---------- O próprio usuário (qualquer perfil) ----------

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
}
