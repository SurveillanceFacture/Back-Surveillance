using System.Text.RegularExpressions;

namespace SurveillanceAPI.Applications.Transcricao;

// Valida o CONTEÚDO devolvido pela IA (o schema só garante o formato).
// Regra central: todo número do texto precisa existir nos fatos daquele item.
public static class ValidadorTranscricao
{
    private static readonly Regex Numeros =
        new(@"\d+(?:[.,]\d+)*", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly string[] TermosProibidos =
    [
        "provavelmente", "provável", "possivelmente", "talvez",
        "recomend", "sugere", "sugest", "deveria", "corrigir", "causa"
    ];

    private static readonly char[] CaracteresProibidos = ['<', '>', '{', '}', '\n', '\r'];

    public static bool EhValida(
        string? texto,
        IEnumerable<string?> valoresPermitidos,
        int minimo = 15,
        int maximo = 300)
    {
        if (string.IsNullOrWhiteSpace(texto)) return false;

        texto = texto.Trim();
        if (texto.Length < minimo || texto.Length > maximo) return false;
        if (texto.IndexOfAny(CaracteresProibidos) >= 0) return false;

        foreach (var termo in TermosProibidos)
            if (texto.Contains(termo, StringComparison.OrdinalIgnoreCase))
                return false;

        var literais = new HashSet<string>(StringComparer.Ordinal);
        var decimais = new HashSet<decimal>();

        foreach (var permitido in valoresPermitidos)
        {
            if (string.IsNullOrWhiteSpace(permitido)) continue;

            foreach (Match m in Numeros.Matches(permitido))
            {
                literais.Add(m.Value);
                if (FormatoPtBr.TentarLerPtBr(m.Value, out var d)) decimais.Add(d);
            }
        }

        foreach (Match m in Numeros.Matches(texto))
        {
            if (literais.Contains(m.Value)) continue;
            // "12,50" é aceito quando o fato tem "12,5" (mesmo valor, outra formatação).
            if (FormatoPtBr.TentarLerPtBr(m.Value, out var d) && decimais.Contains(d)) continue;
            return false;
        }

        return true;
    }
}
