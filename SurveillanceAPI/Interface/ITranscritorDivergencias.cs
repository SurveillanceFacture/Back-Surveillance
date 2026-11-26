using SurveillanceAPI.Applications.Correcao;
using SurveillanceAPI.Applications.Transcricao;

namespace SurveillanceAPI.Interface;

public interface ITranscritorDivergencias
{
    Task<ResultadoTranscricao> TranscreverAsync(AnaliseDocumento analise, CancellationToken ct = default);
}