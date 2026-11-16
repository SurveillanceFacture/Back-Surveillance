using System.Text.Json.Serialization;

namespace SurveillanceAPI.Dto.ApiUpload.XML
{
    public class EventosNotaFiscalDto
    {
        [JsonPropertyName("ChaveAcesso")]
        public string ChaveAcesso { get; init; } = string.Empty;

        [JsonPropertyName("tipoEvento")]
        public string TipoEvento { get; init; } = string.Empty;

        [JsonPropertyName("descricao")]
        public string Descricao { get; init; } = string.Empty;

        [JsonPropertyName("CNPJ")]
        public string Cnpj { get; init; } = string.Empty;

        [JsonPropertyName("DataHoraEvento")]
        public DateTimeOffset DataHoraEvento { get; init; }

        [JsonPropertyName("Justificativa")]
        public string Justificativa { get; init; } = string.Empty;

        [JsonPropertyName("CodigoStatus")]
        public string CodigoStatus { get; init; } = string.Empty;

        [JsonPropertyName("Motivo")]
        public string Motivo { get; init; } = string.Empty; 


    }

}

