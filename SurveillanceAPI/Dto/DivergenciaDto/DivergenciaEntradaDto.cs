using System.ComponentModel.DataAnnotations;

namespace SurveillanceAPI.Dto.DivergenciaDto;

public class DivergenciaEntradaDto
{
    [Required]
    [StringLength(120, MinimumLength = 1)]
    public string Id { get; private set; }  = string.Empty;
    
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Campo { get; private set; } = string.Empty;
    
    [Required]
    [StringLength(80,MinimumLength = 1)]
    public string Tipo { get; private set; } = string.Empty;
    
    [Required]
    [StringLength(300)]
    public string? CaminhoXml { get; private set; }
    
    [Required]
    [StringLength(300)]
    public string? CaminhoJson { get; private set; }
    
    [StringLength(1000)]
    public string? DescricaoTecnica { get; private set; }
    
    [StringLength(500)]
    public string? ValorXml { get; set; }

    [StringLength(500)]
    public string? ValorJson { get; set; }
}   