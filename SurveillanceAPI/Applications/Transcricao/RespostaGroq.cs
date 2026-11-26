namespace SurveillanceAPI.Applications.Transcricao;

public sealed record RespostaGroq(
    string Conteudo,
    string? MotivoFim,
    int? TokensEntrada,
    int? TokensSaida);