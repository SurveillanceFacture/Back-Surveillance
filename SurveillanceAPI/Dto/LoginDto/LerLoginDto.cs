namespace SurveillanceAPI.Dto;

public class LerLoginDto
{
    public string Token { get; init; } = null!;
    public string Nome { get; init; } = null!;
    public string TipoUsuario { get; init; } = null!;
    public Guid UsuarioId { get; init; }
    public DateTime ExpiraEm { get; init; }
}