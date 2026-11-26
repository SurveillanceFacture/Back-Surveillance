using System.ComponentModel.DataAnnotations;

namespace SurveillanceAPI.Dto.UsuarioDto;

public class AlterarMeuEmailDto
{
    [Required(ErrorMessage = "A senha atual é obrigatória.")]
    [StringLength(128)]
    public string SenhaAtual { get; init; } = null!;

    [Required(ErrorMessage = "O novo e-mail é obrigatório.")]
    [StringLength(254)]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string NovoEmail { get; init; } = null!;
}