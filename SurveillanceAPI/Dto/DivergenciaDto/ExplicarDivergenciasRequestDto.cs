using System.ComponentModel.DataAnnotations;

namespace SurveillanceAPI.Dto.DivergenciaDto;

public class ExplicarDivergenciasRequestDto
{
    [Required] [MinLength(1)] 
    public List<DivergenciaEntradaDto> Divergencias { get; private set; } = new();
}