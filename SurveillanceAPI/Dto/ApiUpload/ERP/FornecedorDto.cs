using System.Text.Json.Serialization;

namespace SurveillanceAPI.Dto.ApiUpload.ERP
{
    public class FornecedorDto
    {
        [JsonPropertyName("cnpj")]
        public string Cnpj { get; init; } = string.Empty;

        [JsonPropertyName("razao_social")]
        public string RazaoSocial { get; init; } = string.Empty;

        [JsonPropertyName("categorias")]
        public List<string> Categorias { get; init; } = new();
    }
}
