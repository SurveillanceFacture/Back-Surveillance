using System.ComponentModel.DataAnnotations;

namespace SurveillanceAPI.Dto.UsuarioDto;

public class AlterarMeuNomeDto
{
    [Required(ErrorMessage = "A senha atual é obrigatória.")]
    [StringLength(128)]
    public string SenhaAtual { get; init; } = null!;

    [Required(ErrorMessage = "O novo nome é obrigatório.")]
    [StringLength(150)]
    public string NovoNome { get; init; } = null!;
}