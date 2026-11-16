using System.Text.Json.Serialization;

namespace SurveillanceAPI.Dto.ApiUpload.ERP
{
    public class LojaDto
    {
        [JsonPropertyName("codigo")]
        public string Codigo { get; init; } = string.Empty;

        [JsonPropertyName("nome")]
        public string Nome { get; init; } = string.Empty;

        [JsonPropertyName("cnpj")]
        public string Cnpj { get; init; } = string.Empty;

        [JsonPropertyName("cidade")]
        public string Cidade { get; init; } = string.Empty;

        [JsonPropertyName("uf")]
        public string Uf { get; init; } = string.Empty;
    }
}
