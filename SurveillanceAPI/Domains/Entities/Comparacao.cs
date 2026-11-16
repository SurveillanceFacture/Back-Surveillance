using SurveillanceAPI.Exceptions;

namespace SurveillanceAPI.Domains;

public class Comparacao
{
    public long ComparacaoId { get; private set; }
    public Guid LojaId { get; private set; }
    public Guid AuditorUsuarioId { get; private set; }   
    public string ChaveDocumento { get; private set; } = null!;
    public string HashXml { get; private set; } = null!;
    public StatusComparacao Status { get; private set; }
    public DateTime DataComparacao { get; private set; }

    public Loja Loja { get; private set; } = null!;
    public Usuario Auditor { get; private set; } = null!;

    private readonly List<Divergencia> _divergencias = new();
    public IReadOnlyCollection<Divergencia> Divergencias => _divergencias;

    private Comparacao() { }

    public Comparacao(Guid lojaId, Guid auditorUsuarioId, string chaveDocumento,
                      string hashXml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chaveDocumento);

        if (lojaId == Guid.Empty)
            throw new DomainException("Loja inválida.");

        if (auditorUsuarioId == Guid.Empty)
            throw new DomainException("Auditor inválido.");

        if (chaveDocumento.Trim().Length > 44)
            throw new DomainException("Chave do documento inválida.");
        
        LojaId = lojaId;
        AuditorUsuarioId = auditorUsuarioId;
        ChaveDocumento = chaveDocumento.Trim();
        HashXml = hashXml;
        Status = StatusComparacao.Conforme;
        DataComparacao = DateTime.UtcNow;
    }

    public void AdicionarDivergencia(string campo, string? valorXml,
                                     GravidadeDivergencia gravidade)
    {
        _divergencias.Add(new Divergencia(campo,valorXml, gravidade));
        Status = StatusComparacao.ComDivergencias;
    }
}
