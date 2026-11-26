using System.Globalization;
using System.Text.Json.Serialization;

namespace SurveillanceAPI.Applications.Transcricao;

public static class TiposFato
{
    public const string ValorDiferente = "ValorDiferente";
    public const string ItemAusenteNoErp = "ItemAusenteNoErp";
}

// Fato já calculado e verificado em C#. É a ÚNICA coisa que a IA enxerga: nenhum texto livre da nota
// (descrição de produto, razão social etc.) entra aqui.
public sealed record FatoDivergencia
{
    public required string Ref { get; init; }
    public required string Tipo { get; init; }
    public required string Campo { get; init; }
    public required string Gravidade { get; init; }
    public int? NumeroItem { get; init; }
    public string? Ean { get; init; }
    public string? ValorXml { get; init; }
    public string? ValorErp { get; init; }
    public string? Diferenca { get; init; }
    public string? Sentido { get; init; }

    // Proposta de correção (sempre: ajustar o ERP para o valor do XML).
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool PropostaAlterarErp { get; init; }
    public string? TotalErpAtual { get; init; }
    public string? TotalErpProposto { get; init; }

    // Frase fixa escrita em código explicando por que não há correção automática.
    public string? SemCorrecao { get; init; }

    // Tudo que a transcrição tem permissão de citar numericamente.
    public IEnumerable<string?> ValoresPermitidos()
    {
        yield return NumeroItem?.ToString(CultureInfo.InvariantCulture);
        yield return Ean;
        yield return ValorXml;
        yield return ValorErp;
        yield return Diferenca;
        yield return TotalErpAtual;
        yield return TotalErpProposto;
    }
}