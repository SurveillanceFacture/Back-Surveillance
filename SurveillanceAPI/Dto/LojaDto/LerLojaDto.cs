namespace SurveillanceAPI.Dto.LojaDto;


public class UsuarioResumoDto
{
    public Guid UsuarioId { get; init; }
    public string Nome { get; init; } = null!;
}
public class LerLojaDto
{
    public Guid LojaId { get; init; }
    public string RazaoSocial { get; init; } = null!;
    public string Cnpj { get; init; } = null!;
    public bool Status { get; init; }
    public DateTime DataCriacao { get; init; }
    public UsuarioResumoDto Auditor { get; init; } = null!;
    public UsuarioResumoDto Gerente { get; init; } = null!;
}