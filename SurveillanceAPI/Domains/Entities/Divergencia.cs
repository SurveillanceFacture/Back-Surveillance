using SurveillanceAPI.Exceptions;

namespace SurveillanceAPI.Domains;

public class Divergencia
{
    public Guid DivergenciaId { get; private set; }
    public Guid ComparacaoId { get; private set; }
    public string CampoComparado { get; private set; } = null!;
    public string? ValorXml { get; private set; }
    public string? ValorJson { get; private set; }
    public GravidadeDivergencia Gravidade { get; private set; }
    public StatusDivergencia Status { get; private set; }
    public DateTime DataDeteccao { get; private set; }
    public Guid? ResolvidoPorUsuarioId { get; private set; }
    public DateTime? DataResolucao { get; private set; }
    public string? Observacao { get; private set; }

    public Comparacao Comparacao { get; private set; } = null!;
    public Usuario? ResolvidoPorUsuario { get; private set; }

    private readonly List<HistoricoDivergencia> _historico = new();
    public IReadOnlyCollection<HistoricoDivergencia> Historico => _historico;

    private Divergencia() { }

    internal Divergencia(Guid comparacaoId, string campo, string? valorXml,
                         string? valorJson, GravidadeDivergencia gravidade)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(campo);

        if (comparacaoId == Guid.Empty)
            throw new DomainException("Comparação inválida.");

        if (campo.Trim().Length > 200)
            throw new DomainException("Nome do campo muito longo.");

        if (!Enum.IsDefined(gravidade))
            throw new DomainException("Gravidade inválida.");

        if (valorXml == valorJson)
            throw new DomainException("Os valores são iguais, não há divergência.");

        DivergenciaId = Guid.NewGuid();
        ComparacaoId = comparacaoId;
        CampoComparado = campo.Trim();
        ValorXml = Truncar(valorXml);
        ValorJson = Truncar(valorJson);
        Gravidade = gravidade;
        Status = StatusDivergencia.Aberta;
        DataDeteccao = DateTime.UtcNow;
    }

    public void IniciarAnalise(Guid usuarioId)
    {
        if (Status != StatusDivergencia.Aberta)
            throw new DomainException("Só divergências abertas podem entrar em análise.");

        if (usuarioId == Guid.Empty)
            throw new DomainException("Usuário inválido.");

        RegistrarMudanca(StatusDivergencia.EmAnalise, usuarioId, null);
    }

    public void Resolver(Guid usuarioId, string? observacao = null) =>
        Fechar(StatusDivergencia.Resolvida, usuarioId, observacao);

    public void Justificar(Guid usuarioId, string observacao)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(observacao);
        Fechar(StatusDivergencia.Justificada, usuarioId, observacao);
    }

    private void Fechar(StatusDivergencia novoStatus, Guid usuarioId, string? observacao)
    {
        if (Status is StatusDivergencia.Resolvida or StatusDivergencia.Justificada)
            throw new DomainException("Divergência já foi encerrada.");

        if (usuarioId == Guid.Empty)
            throw new DomainException("Usuário inválido.");

        if (observacao is { Length: > 1000 })
            throw new DomainException("Observação muito longa.");

        ResolvidoPorUsuarioId = usuarioId;
        DataResolucao = DateTime.UtcNow;
        Observacao = observacao?.Trim();
        RegistrarMudanca(novoStatus, usuarioId, observacao);
    }

    private void RegistrarMudanca(StatusDivergencia novo, Guid usuarioId, string? observacao)
    {
        _historico.Add(new HistoricoDivergencia(DivergenciaId, Status, novo, usuarioId, observacao));
        Status = novo;
    }

    private static string? Truncar(string? valor) =>
        valor is null ? null : (valor.Length > 500 ? valor[..500] : valor);
}