using System.Text.Json.Serialization;

namespace SurveillanceAPI.Dto.ApiUpload.ERP
{
    public class ItemEstoqueDto
    {
        [JsonPropertyName("codigo_produto")]
        public string CodigoProduto { get; init; } = string.Empty;

        [JsonPropertyName("quantidade")]
        public decimal Quantidade { get; init; }
    }
}
