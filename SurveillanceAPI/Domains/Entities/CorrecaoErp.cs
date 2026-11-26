using System.Text.Json;
using SurveillanceAPI.Exceptions;

namespace SurveillanceAPI.Domains;

public enum StatusCorrecaoErp
{
    Proposta = 1,
    Aplicada = 2
}

// Uma alteração em um campo de um item do JSON do ERP. "Derivada" = efeito calculado (ex.: valor_total).
public sealed record AlteracaoErp(string Campo, string CampoComparado, string Antes, string Depois, bool Derivada);

// Proposta de correção de UM item do ERP. Nasce como Proposta, presa ao usuário, com validade e uso único.
public class CorrecaoErp
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public Guid CorrecaoErpId { get; private set; }
    public long ComparacaoId { get; private set; }
    public Guid LojaId { get; private set; }
    public Guid UsuarioId { get; private set; }

    public string IdLancamento { get; private set; } = null!;
    public int IndiceItem { get; private set; }
    public int NumeroItem { get; private set; }
    public string? Ean { get; private set; }

    public string AlteracoesJson { get; private set; } = null!;
    public string Explicacao { get; private set; } = null!;
    public bool ExplicacaoGeradaPorIa { get; private set; }

    public string HashErp { get; private set; } = null!;
    public string? HashErpDepois { get; private set; }

    public StatusCorrecaoErp Status { get; private set; }
    public DateTime CriadaEm { get; private set; }
    public DateTime ExpiraEm { get; private set; }
    public DateTime? AplicadaEm { get; private set; }
    public Guid? AplicadaPorUsuarioId { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    public Comparacao Comparacao { get; private set; } = null!;

    private CorrecaoErp() { }

    public CorrecaoErp(
        Comparacao comparacao,
        Guid lojaId,
        Guid usuarioId,
        string idLancamento,
        int indiceItem,
        int numeroItem,
        string? ean,
        IReadOnlyList<AlteracaoErp> alteracoes,
        string hashErp,
        string explicacao,
        bool explicacaoGeradaPorIa,
        DateTime agora,
        TimeSpan validade)
    {
        ArgumentNullException.ThrowIfNull(comparacao);
        ArgumentException.ThrowIfNullOrWhiteSpace(idLancamento);
        ArgumentException.ThrowIfNullOrWhiteSpace(hashErp);

        if (lojaId == Guid.Empty) throw new DomainException("Loja inválida.");
        if (usuarioId == Guid.Empty) throw new DomainException("Usuário inválido.");
        if (indiceItem < 0) throw new DomainException("Índice de item inválido.");
        if (alteracoes.Count == 0) throw new DomainException("Uma correção precisa de ao menos uma alteração.");

        CorrecaoErpId = Guid.NewGuid();
        Comparacao = comparacao;
        LojaId = lojaId;
        UsuarioId = usuarioId;
        IdLancamento = idLancamento.Trim();
        IndiceItem = indiceItem;
        NumeroItem = numeroItem;
        Ean = ean?.Trim();
        AlteracoesJson = JsonSerializer.Serialize(alteracoes, JsonOpts);
        Explicacao = explicacao.Length > 4000 ? explicacao[..4000] : explicacao;
        ExplicacaoGeradaPorIa = explicacaoGeradaPorIa;
        HashErp = hashErp;
        Status = StatusCorrecaoErp.Proposta;
        CriadaEm = agora;
        ExpiraEm = agora + validade;
    }

    public IReadOnlyList<AlteracaoErp> Alteracoes() =>
        JsonSerializer.Deserialize<List<AlteracaoErp>>(AlteracoesJson, JsonOpts) ?? new List<AlteracaoErp>();

    public void ValidarAplicavel(DateTime agora)
    {
        if (Status == StatusCorrecaoErp.Aplicada)
            throw new DomainException.ConflitoException("Esta proposta já foi aplicada.");

        if (agora > ExpiraEm)
            throw new DomainException("A proposta expirou. Gere uma nova proposta.");
    }

    public void MarcarAplicada(Guid usuarioId, string hashErpDepois, DateTime agora)
    {
        ValidarAplicavel(agora);

        if (usuarioId == Guid.Empty) throw new DomainException("Usuário inválido.");
        ArgumentException.ThrowIfNullOrWhiteSpace(hashErpDepois);

        Status = StatusCorrecaoErp.Aplicada;
        AplicadaPorUsuarioId = usuarioId;
        AplicadaEm = agora;
        HashErpDepois = hashErpDepois;
    }
}
