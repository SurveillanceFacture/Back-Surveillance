namespace SurveillanceAPI.Domains;


public class HistoricoDivergencia
{
    public long HistoricoDivergenciaId { get; private set; }
    public long DivergenciaId { get; private set; }
    public StatusDivergencia StatusAnterior { get; private set; }
    public StatusDivergencia StatusNovo { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string? Observacao { get; private set; }
    public DateTime DataAlteracao { get; private set; }

    public Divergencia Divergencia { get; private set; } = null!;
    public Usuario Usuario { get; private set; } = null!;

    private HistoricoDivergencia() { }

    internal HistoricoDivergencia( StatusDivergencia anterior,
        StatusDivergencia novo, Guid usuarioId, string? observacao)
    {
        StatusAnterior = anterior;
        StatusNovo = novo;
        UsuarioId = usuarioId;
        Observacao = observacao?.Trim();
        DataAlteracao = DateTime.UtcNow;
    }
}