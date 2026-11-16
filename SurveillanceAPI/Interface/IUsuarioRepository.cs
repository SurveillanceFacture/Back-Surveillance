using SurveillanceAPI.Domains;

namespace SurveillanceAPI.Interface;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct);
    
    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct);

    Task<bool> EmailExisteAsync(string email, Guid? ignorarUsuarioId, CancellationToken ct);

    Task<(IReadOnlyList<Usuario> Itens, int Total)> ListarAsync(
        string? busca, int? tipoUsuarioId, bool? ativo, int pagina, int tamanho, CancellationToken ct);

    void Adicionar(Usuario usuario);

    Task SalvarAsync(CancellationToken ct);
}