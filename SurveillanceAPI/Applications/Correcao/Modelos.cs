using SurveillanceAPI.Domains;
using SurveillanceAPI.Dto.ApiUpload.XML;

namespace SurveillanceAPI.Applications.Correcao;

// Endereço estável de um item no JSON do ERP: movimentacoes[id_lancamento].itens[indice].
public sealed record ErpLocalizador(string IdLancamento, int IndiceItem);

public sealed record DivergenciaAnalisada(
    DivergenciasDto Divergencia,
    ErpLocalizador? Alvo,
    AlteracaoErp? Alteracao,
    string? MotivoSemCorrecao);

public sealed record PlanoItem(
    ErpLocalizador Alvo,
    int NumeroItem,
    string? Ean,
    IReadOnlyList<AlteracaoErp> Alteracoes);

public sealed record AnaliseDocumento(
    IReadOnlyList<DivergenciaAnalisada> Divergencias,
    IReadOnlyList<PlanoItem> Planos);

// Frases fixas escritas em código (nunca vêm do XML), por isso podem ir à IA sem risco de injeção.
public static class MotivosSemCorrecao
{
    public const string ItemAusente = "o item precisa ser lançado manualmente no ERP";
    public const string EanRepetido = "o EAN se repete na nota ou no ERP e a correção automática poderia atingir o item errado";
    public const string SemLocalizador = "o lançamento do ERP não tem identificador único no arquivo";
    public const string NcmInvalido = "o NCM da nota não tem 8 dígitos";
    public const string ValorInvalido = "o valor da nota é zero ou negativo";
    public const string TotalNaoConfere = "o valor total da nota não confere com quantidade × valor unitário";
    public const string SemRegra = "não há regra de correção automática para este campo";
}

public static class Digitos
{
    public static string Somente(string? valor)
    {
        if (string.IsNullOrEmpty(valor)) return string.Empty;

        var buffer = new char[valor.Length];
        var n = 0;
        foreach (var c in valor)
            if (c is >= '0' and <= '9') buffer[n++] = c;

        return new string(buffer, 0, n);
    }
}

public static class GravidadeParser
{
    public static GravidadeDivergencia Converter(string? gravidade) =>
        gravidade?.Trim().ToLowerInvariant() switch
        {
            "critica" or "crítica" => GravidadeDivergencia.Critica,
            "alta" => GravidadeDivergencia.Alta,
            "baixa" => GravidadeDivergencia.Baixa,
            _ => GravidadeDivergencia.Media
        };
}
