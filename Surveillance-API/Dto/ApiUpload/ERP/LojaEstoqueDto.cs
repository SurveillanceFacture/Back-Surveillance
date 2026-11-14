using System.Text.Json.Serialization;

namespace Surveillance_API.Dto.ApiUpload.ERP
{
    public class LojaEstoqueDto
    {
        [JsonPropertyName("Loja")]
        public string Loja { get; init; } = string.Empty;

        [JsonPropertyName("itens")]
        public List<ItemEstoqueDto> Itens { get; init; } = new();    

    }
}
