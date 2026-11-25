using SurveillanceAPI.Exceptions;

namespace SurveillanceAPI.Domains;

public class Divergencia
{
    public Guid DivergenciaId { get; private set; }
    public long ComparacaoId { get; private set; }
    public int? NumeroItem { get; private set; }
    public string? CodigoProduto { get; private set; }
    public string? Ean { get; private set; }
    public string TipoDivergencia { get; private set; } = string.Empty;
    public string? CampoComparado { get; private set; }
    public string? ValorXml { get; private set; }
    public string? ValorJson { get; private set; }
    public decimal ImpactoFinanceiro { get; private set; }
    public GravidadeDivergencia Gravidade { get; private set; }
    public StatusDivergencia Status { get; private set; }
    public string? Descricao { get; private set; }
    public string? AcaoRecomendada { get; private set; }
    public DateTime DataDeteccao { get; private set; }
    public Guid? ResolvidoPorUsuarioId { get; private set; }
    public DateTime? DataResolucao { get; private set; }
    public string? Observacao { get; private set; }

    public Comparacao Comparacao { get; private set; } = null!;
    public Usuario? ResolvidoPorUsuario { get; private set; }

    private readonly List<HistoricoDivergencia> _historico = new();
    public IReadOnlyCollection<HistoricoDivergencia> Historico => _historico;

    private Divergencia() { }

    internal Divergencia(
        long comparacaoId,
        string tipoDivergencia,
        string campo,
        string? valorXml,
        string? valorErp,
        decimal impactoFinanceiro,
        GravidadeDivergencia gravidade,
        int? numeroItem = null,
        string? codigoProduto = null,
        string? ean = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tipoDivergencia);
        ArgumentException.ThrowIfNullOrWhiteSpace(campo);

        if (campo.Trim().Length > 200)
            throw new DomainException("Nome do campo muito longo.");

        if (impactoFinanceiro < 0)
            throw new DomainException("Impacto financeiro não pode ser negativo.");

        if (!Enum.IsDefined(gravidade))
            throw new DomainException("Gravidade inválida.");

        DivergenciaId = Guid.NewGuid();
        ComparacaoId = comparacaoId;
        TipoDivergencia = tipoDivergencia.Trim();
        CampoComparado = campo.Trim();
        ValorXml = Truncar(valorXml);
        ValorJson = Truncar(valorErp);
        ImpactoFinanceiro = Math.Round(impactoFinanceiro, 2);
        Gravidade = gravidade;
        NumeroItem = numeroItem;
        CodigoProduto = Truncar(codigoProduto);
        Ean = Truncar(ean);
        Status = StatusDivergencia.Aberta;
        DataDeteccao = DateTime.UtcNow;
    }

    public void PreencherAnaliseIA(string? descricao, string? acaoRecomendada)
    {
        Descricao = Truncar(descricao);
        AcaoRecomendada = Truncar(acaoRecomendada);
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
        _historico.Add(new HistoricoDivergencia(Status, novo, usuarioId, observacao));
        Status = novo;
    }

    private static string? Truncar(string? valor) =>
        valor is null ? null : (valor.Length > 500 ? valor[..500] : valor);
}