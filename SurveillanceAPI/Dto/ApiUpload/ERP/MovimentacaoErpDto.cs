using System.Text.Json.Serialization;

namespace SurveillanceAPI.Dto.ApiUpload.ERP
{
    public class MovimentacaoErpDto
    {
        [JsonPropertyName("loja")]
        public LojaDto Loja { get; init; } = new();

        [JsonPropertyName("competencia")]
        public DateOnly Competencia { get; init; }

        [JsonPropertyName("sistema")]
        public string Sistema { get; init; } = string.Empty;

        [JsonPropertyName("gerado_em")]
        public DateTime Gerado_em { get; init; }

        [JsonPropertyName("total_movimentacoes")]
        public int TotalMovimentacoes { get; init; }

        [JsonPropertyName("movimentacoes")]
        public List<LancamentoMovimentacaoDto> Movimentacoes { get; init; } = new();

    }
}
