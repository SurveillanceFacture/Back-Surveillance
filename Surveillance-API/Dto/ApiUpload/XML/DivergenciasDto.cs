using System.Text.Json.Serialization;

namespace Surveillance_API.Dto.ApiUpload.XML
{
    public class DivergenciasDto
    {

        [JsonPropertyName("comparacaoid")]
        public Guid ComparacaoId { get; init;  }

        [JsonPropertyName("numero_item")]
        public int? NumeroItem { get; init; }

        [JsonPropertyName("codigo_produto")]
        public string? CodigoProduto { get; init; } = string.Empty;

        [JsonPropertyName("tipo_divergencia")]
        public string TipoDivergencia { get; init; } = string.Empty;

        [JsonPropertyName("acao_recomendada")]
        public string AcaoRecomendada { get; set; } = string.Empty;

        [JsonPropertyName("descricao")]
        public string Descricao { get; set; } = string.Empty;

        [JsonPropertyName("ean")]
        public string? Ean { get; init; } = string.Empty;

        [JsonPropertyName("campo_comparado")]
        public string CampoComparado { get; init; } = string.Empty;

        [JsonPropertyName("valor_xml")]
        public string? ValorXML { get; init; } = string.Empty;

        [JsonPropertyName("valor_erp")]
        public string? ValorERP { get; init; } = string.Empty;

        [JsonPropertyName("gravidade")]
        public string Gravidade { get; init; } = string.Empty;

        [JsonPropertyName("data_deteccao")]
        public DateTimeOffset DataDeteccao { get; init; } = DateTimeOffset.UtcNow;
    

   }
}
