using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SurveillanceAPI.Dto.DivergenciaDto;

public class DivergenciaExplicadaDto
{
    public string Id { get; set; } = string.Empty;
    public string Transcricao { get; set; } = string.Empty;
}