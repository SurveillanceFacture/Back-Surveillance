using System.Text.Json.Serialization;

namespace Surveillance_API.Dto.ApiUpload.ERP
{
    public class DocumentoMovimentacaoDto
    {
        [JsonPropertyName("chave")]
        public string Chave { get; init; } = string.Empty;

        [JsonPropertyName("modelo")]
        public int Modelo { get; init; }

        [JsonPropertyName("serie")]
        public int Serie { get; init; }

        [JsonPropertyName("numero")]
        public int Numero { get; init; }

        [JsonPropertyName("data_emissao")]
        public DateOnly DataEmissao { get; init; }
    }
}
