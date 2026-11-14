using System.Text.Json.Serialization;

namespace Surveillance_API.Dto.ApiUpload.XML
{
    public class ItemNotaFiscalDto
    {
        [JsonPropertyName("numero_item")]
        public int NumeroItem { get; init; }

        [JsonPropertyName("codigo_produto")]
        public string CodigoProduto { get; init; } = string.Empty;

        [JsonPropertyName("ean")]
        public string Ean { get; init; } = string.Empty;

        [JsonPropertyName("ncm")]
        public string NCM { get; init; } = string.Empty;

        [JsonPropertyName("cfop")]
        public string CFOP { get; init; } = string.Empty;

        [JsonPropertyName("descricao")]
        public string Descricao { get; init; } = string.Empty;

        [JsonPropertyName("unidade")]
        public string Unidade { get; init; } = string.Empty;

        [JsonPropertyName("quantidade")]
        public decimal Quantidade { get; init; }

        [JsonPropertyName("valor_unitario")]
        public decimal ValorUnitario { get; init; }

        [JsonPropertyName("valor_total")]
        public decimal ValorTotal { get; init; }
    }
}
