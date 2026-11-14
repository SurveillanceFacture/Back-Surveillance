using System.Text.Json.Serialization;

namespace Surveillance_API.Dto.ApiUpload.ERP
{
    public class LancamentoMovimentacaoDto
    {
        [JsonPropertyName("id_lancamento")]
        public string IdLancamento { get; init; } = string.Empty;

        [JsonPropertyName("tipo")]
        public string Tipo { get; init; } = string.Empty;

        [JsonPropertyName("data_lancamento")]
        public DateTimeOffset DataLancamento { get; init; }

        [JsonPropertyName("documento")]
        public DocumentoMovimentacaoDto Documento { get; init; } = new();

        [JsonPropertyName("parceiro_cnpj")]
        public string ?ParceiroCnpj { get; init; }

        [JsonPropertyName("itens")]
        public List<ItemMovimentacaoErpDto> Itens { get; init; } = new();
    }
}
