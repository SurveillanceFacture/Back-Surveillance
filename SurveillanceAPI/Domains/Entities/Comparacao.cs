using SurveillanceAPI.Exceptions;

namespace SurveillanceAPI.Domains;

public class Comparacao
{
    public Guid ComparacaoId { get; private set; }
    public Guid LojaId { get; private set; }
    public Guid AuditorUsuarioId { get; private set; }   // auditor NAQUELE momento
    public string ChaveDocumento { get; private set; } = null!;
    public string HashXml { get; private set; } = null!;
    public string HashJson { get; private set; } = null!;
    public string CaminhoXml { get; private set; } = null!;
    public string CaminhoJson { get; private set; } = null!;
    public StatusComparacao Status { get; private set; }
    public DateTime DataComparacao { get; private set; }

    public Loja Loja { get; private set; } = null!;
    public Usuario Auditor { get; private set; } = null!;

    private readonly List<Divergencia> _divergencias = new();
    public IReadOnlyCollection<Divergencia> Divergencias => _divergencias;

    private Comparacao() { }

    public Comparacao(Guid lojaId, Guid auditorUsuarioId, string chaveDocumento,
                      string hashXml, string hashJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chaveDocumento);
        ArgumentException.ThrowIfNullOrWhiteSpace(hashXml);
        ArgumentException.ThrowIfNullOrWhiteSpace(hashJson);

        if (lojaId == Guid.Empty)
            throw new DomainException("Loja inválida.");

        if (auditorUsuarioId == Guid.Empty)
            throw new DomainException("Auditor inválido.");

        if (chaveDocumento.Trim().Length > 44)
            throw new DomainException("Chave do documento inválida.");

        ComparacaoId = Guid.NewGuid();
        LojaId = lojaId;
        AuditorUsuarioId = auditorUsuarioId;
        ChaveDocumento = chaveDocumento.Trim();
        HashXml = hashXml;
        HashJson = hashJson;    
        Status = StatusComparacao.Conforme;
        DataComparacao = DateTime.UtcNow;
    }

    public void AdicionarDivergencia(string campo, string? valorXml, string? valorJson,
                                     GravidadeDivergencia gravidade)
    {
        _divergencias.Add(new Divergencia(ComparacaoId, campo, valorXml, valorJson, gravidade));
        Status = StatusComparacao.ComDivergencias;
    }
}