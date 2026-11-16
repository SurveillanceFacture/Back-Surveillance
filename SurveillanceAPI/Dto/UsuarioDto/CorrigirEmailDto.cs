using System.ComponentModel.DataAnnotations;

namespace SurveillanceAPI.Dto.UsuarioDto;

public class CorrigirEmailDto
{
    [Required(ErrorMessage = "O novo e-mail é obrigatório.")]
    [StringLength(254)]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string NovoEmail { get; init; } = null!;
}