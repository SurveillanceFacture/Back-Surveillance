namespace SurveillanceAPI.Dto.UsuarioDto;

public class LerUsuarioDto
{
    public Guid UsuarioId { get; init; }
    public string NomeUsuario { get; init; }
    public string Email { get; init; }
    public int TipoUsuarioId { get; init; }
    public string TipoUsuario { get; init; }
    public bool Status { get; init; }
    public DateTime? DataCriacao { get; init; }
    public DateTime? UltimoAcesso {get; init; }
}