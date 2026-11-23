using SurveillanceAPI.Dto.DivergenciaDto;

namespace SurveillanceAPI.Interface;

public interface IGroqDivergenciaService
{
    Task<ExplicarDivergenciasResponseDto> ExplicarAsync(ExplicarDivergenciasRequestDto request,
        CancellationToken cancellationToken = default);
}