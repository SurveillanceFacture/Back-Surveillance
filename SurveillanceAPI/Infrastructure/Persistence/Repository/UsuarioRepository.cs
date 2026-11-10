using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SurveillanceAPI.Domains;
using SurveillanceAPI.Exceptions;
using SurveillanceAPI.Interface;

namespace SurveillanceAPI.Infraestructure.Persistence.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _db;

    public UsuarioRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct) =>
        _db.Usuarios
            .Include(u => u.TipoUsuario)
            .FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct) =>
        _db.Usuarios
            .Include(u => u.TipoUsuario)
            .FirstOrDefaultAsync(u => u.UsuarioId == id, ct);

    public Task<bool> EmailExisteAsync(string email, Guid? ignorarUsuarioId, CancellationToken ct) =>
        _db.Usuarios.AnyAsync(
            u => u.Email == email && (ignorarUsuarioId == null || u.UsuarioId != ignorarUsuarioId.Value),
            ct);

    public async Task<(IReadOnlyList<Usuario> Itens, int Total)> ListarAsync(
        string? busca, int? tipoUsuarioId, bool? ativo, int pagina, int tamanho, CancellationToken ct)
    {
        IQueryable<Usuario> query = _db.Usuarios
            .AsNoTracking()
            .Include(u => u.TipoUsuario);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            query = query.Where(u => u.NomeUsuario.Contains(termo) || u.Email.Contains(termo));
        }

        if (tipoUsuarioId.HasValue)
            query = query.Where(u => u.TipoUsuarioId == tipoUsuarioId.Value);

        if (ativo.HasValue)
            query = query.Where(u => u.StatusUsuario == ativo.Value);

        var total = await query.CountAsync(ct);

        // OrderBy por nome + id: ordem estável, senão a paginação pode repetir ou pular itens
        var itens = await query
            .OrderBy(u => u.NomeUsuario)
            .ThenBy(u => u.UsuarioId)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync(ct);

        return (itens, total);
    }

    public void Adicionar(Usuario usuario) => _db.Usuarios.Add(usuario);

    public async Task SalvarAsync(CancellationToken ct)
    {
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new DomainException("Já existe um registro com esses dados.");
        }
    }
}
