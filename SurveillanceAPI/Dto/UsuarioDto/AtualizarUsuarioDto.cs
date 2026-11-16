using System.ComponentModel.DataAnnotations;

namespace SurveillanceAPI.Dto.UsuarioDto;

public class AtualizarUsuarioDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(150)]
    public string NomeUsuario { get; init; } = null!;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(254)]
    public string Email { get; init; } = null!;

    [Range(1, int.MaxValue, ErrorMessage = "Tipo de usuário inválido.")]
    public int TipoUsuarioId { get; init; }
}