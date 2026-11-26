namespace SurveillanceAPI.Applications.Transcricao;

public sealed record TextoItem(string Texto, bool PorIa);

// Itens[i] corresponde a AnaliseDocumento.Divergencias[i].
public sealed record ResultadoTranscricao(
    IReadOnlyList<TextoItem> Itens,
    string Resumo,
    bool ResumoPorIa);