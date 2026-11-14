using System.Globalization;
using System.Text.Json.Serialization;

namespace Surveillance_API.Dto.ApiUpload.ERP
{
    public class ProdutoErpDto
    {
        [JsonPropertyName("codigo")]
        public string Codigo { get; init; } = string.Empty;

        [JsonPropertyName("Ean")]
        public string Ean { get; init; } = string.Empty;

        [JsonPropertyName("descricao")]
        public string Descricao { get; init; } = string.Empty;

        [JsonPropertyName("ncm")]
        public string Ncm { get; init; } = string.Empty;

        [JsonPropertyName("unidade_estoque")]
        public string unidadeEstoque { get; init; } = string.Empty;

        [JsonPropertyName("embalagem_compra")]
        public EmbalagemCompraDto embalagemCompra { get; init; } = new();

        [JsonPropertyName("preco_venda")]
        public decimal PrecoVenda { get; init; }

        [JsonPropertyName("custo_referencia")]
        public decimal CustoReferencia { get; init; }

        [JsonPropertyName("categoria")]
        public string Categoria { get; init; } = string.Empty;
    }
}
