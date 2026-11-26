using System.ComponentModel.DataAnnotations;

namespace SurveillanceAPI.Dto;

public class EsqueciSenhaDto
{
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(254)]
    public string Email { get; init; } = null!;
}