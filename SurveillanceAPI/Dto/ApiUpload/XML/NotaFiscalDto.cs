using SurveillanceAPI.Dto.ApiUpload.XML;
using System.Text.Json.Serialization;

namespace SurveillanceAPI.Dto.ApiUpload.Xml;

public class NotaFiscalDto
{
    [JsonPropertyName("chave_acesso")]
    public string ChaveAcesso { get; init; } = string.Empty;

    [JsonPropertyName("numero_nota")]
    public int NumeroNota { get; init; } 

    [JsonPropertyName("serie")]
    public int Serie { get; init; } 

    [JsonPropertyName("data_emissao")]
    public DateTimeOffset DataEmissao { get; init; } 

    [JsonPropertyName("cnpj_emitente")]
    public string CnpjEmitente { get; init; } = string.Empty; 

    [JsonPropertyName("cnpj_destinatario")]
    public string CnpjDestinatario { get; init; } = string.Empty; 

    [JsonPropertyName("valor_total")]
    public decimal ValorTotal { get; init; } 

    [JsonPropertyName("itens")]
    public List<ItemNotaFiscalDto> Itens { get; init; } = new();
}