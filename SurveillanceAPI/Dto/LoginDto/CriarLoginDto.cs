using System.ComponentModel.DataAnnotations;
using SurveillanceAPI.Exceptions;

namespace SurveillanceAPI.Dto;

public class CriarLoginDto
{
    [Required(ErrorMessage = "O e-mail eh obrigatorio")]
    [EmailAddress(ErrorMessage = "E-mail invalido")]
    [StringLength(255)]
    public string Email { get; init; } = null!;
    [Required(ErrorMessage = "Senha eh obrigatorio")]
    [StringLength(128)]
    public string Senha { get; init; } = null!;
}