using System.Text.Json.Serialization;

namespace Surveillance_API.Dto.ApiUpload.ERP
{
    public class ItemMovimentacaoErpDto
    {
        [JsonPropertyName("codigo_produto")]
        public string CodigoProduto { get; init; } = string.Empty;

        [JsonPropertyName("quantidade")]
        public decimal Quantidade { get; init; }

        [JsonPropertyName("unidade")]
        public string Unidade { get; init; } = string.Empty;

        [JsonPropertyName("ean")]
        public string Ean { get; init; } = string.Empty;

        [JsonPropertyName("ncm")]
        public string NCM { get; init; } = string.Empty;

        [JsonPropertyName("valor_unitario")]
        public decimal ValorUnitario { get; init; }

        [JsonPropertyName("valor_total")]
        public decimal ValorTotal { get; init; }
    }
}
