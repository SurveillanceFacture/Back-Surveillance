using System.Text.Json.Serialization;

namespace Surveillance_API.Dto.ApiUpload.ERP
{
    public class EstoqueInicialDto
    {
        [JsonPropertyName("data_referencia")]
        public DateTime DataReferencia { get; init; }

        [JsonPropertyName("unidade")]
        public string Unidade { get; init; } = string.Empty;

        [JsonPropertyName("loja")]
        public List<LojaEstoqueDto> Loja { get; init; } = new();
    }
}
