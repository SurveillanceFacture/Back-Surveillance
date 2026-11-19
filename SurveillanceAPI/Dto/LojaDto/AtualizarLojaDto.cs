using System.ComponentModel.DataAnnotations;

namespace SurveillanceAPI.Dto.LojaDto;

public class AtualizarLojaDto
{
    [Required(ErrorMessage = "A razão social é obrigatória.")]
    [StringLength(180)]
    public string RazaoSocial { get; init; } = null!;

    [Required(ErrorMessage = "O auditor é obrigatório.")]
    public Guid? AuditorUsuarioId { get; init; }

    [Required(ErrorMessage = "O gerente é obrigatório.")]
    public Guid? GerenteUsuarioId { get; init; }
}