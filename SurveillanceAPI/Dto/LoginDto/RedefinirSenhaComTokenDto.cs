using System.ComponentModel.DataAnnotations;

namespace SurveillanceAPI.Dto;

public class RedefinirSenhaComTokenDto
{
    [Required(ErrorMessage = "O token é obrigatório.")]
    [StringLength(128)]
    public string Token { get; init; } = null!;

    [Required(ErrorMessage = "A nova senha é obrigatória.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 128 caracteres.")]
    public string NovaSenha { get; init; } = null!;
}