using System.ComponentModel.DataAnnotations;

namespace SurveillanceAPI.Dto.LojaDto;

public class CriarLojaDto
{
    [Required(ErrorMessage = "A razão social é obrigatória.")]
    [StringLength(180)]
    public string RazaoSocial { get; init; } = null!;

    [Required(ErrorMessage = "O CNPJ é obrigatório.")]
    [StringLength(18, MinimumLength = 14, ErrorMessage = "CNPJ inválido.")]   
    public string Cnpj { get; init; } = null!;
    
    
    [Required(ErrorMessage = "O auditor é obrigatório.")]
    public Guid? AuditorUsuarioId { get; init; }

    [Required(ErrorMessage = "O gerente é obrigatório.")]
    public Guid? GerenteUsuarioId { get; init; }
}