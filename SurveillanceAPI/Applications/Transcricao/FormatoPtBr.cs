using System.Globalization;

namespace SurveillanceAPI.Applications.Transcricao;

// Formatação pt-BR independente da cultura da máquina (e de ICU/InvariantGlobalization em containers).
internal static class FormatoPtBr
{
    public static readonly NumberFormatInfo Info = new()
    {
        NumberDecimalSeparator = ",",
        NumberGroupSeparator = "."
    };

    public const string Quantidade = "#,##0.####";
    public const string Moeda = "#,##0.00##";

    public static bool TentarLerPtBr(string texto, out decimal valor) =>
        decimal.TryParse(texto, NumberStyles.Number, Info, out valor);
}