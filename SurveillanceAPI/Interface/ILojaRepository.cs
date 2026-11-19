using SurveillanceAPI.Domains;

namespace SurveillanceAPI.Interface;

public interface ILojaRepository
{
    Task<Loja?> ObterPorIdAsync(Guid id, CancellationToken ct);
    Task<bool> CnpjExisteAsync(string cnpj, Guid? ignorarLojaId, CancellationToken ct);
    Task<bool> GerenteVinculadoAsync(Guid gerenteId, Guid? ignorarLojaId, CancellationToken ct);

    Task<(IReadOnlyList<Loja> Itens, int Total)> ListarAsync(
        string? busca, bool? ativo, Guid? auditorId, Guid? gerenteId,
        int pagina, int tamanho, CancellationToken ct);
    
    Task<IReadOnlyList<Usuario>> ListarGerentesDisponiveisAsync(
        string? busca, Guid? lojaAtualId, int limite, CancellationToken ct);
    
    Task<bool> UsuarioVinculadoAsync(Guid usuarioId, CancellationToken ct);
    
    void Adicionar(Loja loja);
    Task SalvarAsync(CancellationToken ct);
}