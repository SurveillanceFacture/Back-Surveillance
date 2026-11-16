using System.ComponentModel.DataAnnotations;

namespace SurveillanceAPI.Dto.UsuarioDto;

public class CriarUsuarioDto
{
    [Required(ErrorMessage = "O nome e obrigatorio")]
    [StringLength(150)]
    public string NomeUsuario { get; init; } = null!;

    [Required(ErrorMessage = "O e-mail e obrigatorio")]
    [StringLength(254)]
    [EmailAddress(ErrorMessage ="E-mail invalido")]
    public string Email { get; init; } = null!;

    [Required(ErrorMessage = "A senha e obrigatoria")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "A senha entre 8 e 128 caracteres")]
    public string Senha { get; init; } = null!;
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Tipo de usuario invalido")]
    public int TipoUsuarioId { get; init; }

}