using System.Text.Json.Serialization;

namespace SurveillanceAPI.Dto.ApiUpload.ERP
{
    public class EmbalagemCompraDto
    {
        [JsonPropertyName("quantidade")]
        public string Quantidade { get; init; } = string.Empty;

        [JsonPropertyName("fator")]
        public decimal Fator { get; init; } 
    }
}
