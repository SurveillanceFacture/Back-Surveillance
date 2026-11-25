namespace SurveillanceAPI.Dto.DivergenciaDto;

public class ExplicarDivergenciasResponseDto
{
    public string Resumo { get;  set; } = string.Empty;
    public List<DivergenciaExplicadaDto> Itens { get;  set; } = new();
}