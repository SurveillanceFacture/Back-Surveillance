using System.Text.Json.Serialization;

namespace Surveillance_API.Dto.ApiUpload.ERP
{
    public class ItemEstoqueDto
    {
        [JsonPropertyName("codigo_produto")]
        public string CodigoProduto { get; init; } = string.Empty;

        [JsonPropertyName("quantidade")]
        public decimal Quantidade { get; init; }
    }
}
