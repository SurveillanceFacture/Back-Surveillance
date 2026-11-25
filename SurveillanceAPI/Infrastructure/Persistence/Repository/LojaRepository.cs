using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SurveillanceAPI.Domains;
using SurveillanceAPI.Exceptions;
using SurveillanceAPI.Infraestructure.Persistence;
using SurveillanceAPI.Interface;

namespace SurveillanceAPI.Infrastructure.Persistence.Repository;

public class LojaRepository : ILojaRepository
{
    private readonly AppDbContext _db;

    public LojaRepository(AppDbContext db) => _db = db;

    public Task<Loja?> ObterPorIdAsync(Guid id, CancellationToken ct) =>
        _db.Lojas
            .Include(l => l.Auditor)
            .Include(l => l.Gerente)
            .FirstOrDefaultAsync(l => l.LojaId == id, ct);

    public Task<bool> CnpjExisteAsync(string cnpj, Guid? ignorarLojaId, CancellationToken ct) =>
        _db.Lojas.AnyAsync(
            l => l.Cnpj == cnpj && (ignorarLojaId == null || l.LojaId != ignorarLojaId.Value), ct);

    public Task<bool> GerenteVinculadoAsync(Guid gerenteId, Guid? ignorarLojaId, CancellationToken ct) =>
        _db.Lojas.AnyAsync(
            l => l.GerenteUsuarioId == gerenteId && (ignorarLojaId == null || l.LojaId != ignorarLojaId.Value), ct);

    public async Task<(IReadOnlyList<Loja> Itens, int Total)> ListarAsync(
        string? busca, bool? ativo, Guid? auditorId, Guid? gerenteId,
        int pagina, int tamanho, CancellationToken ct)
    {
        IQueryable<Loja> query = _db.Lojas
            .AsNoTracking()
            .Include(l => l.Auditor)
            .Include(l => l.Gerente);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            var cnpjTermo = Loja.NormalizarCnpj(termo);  

            query = query.Where(l =>
                l.RazaoSocial.Contains(termo)
                || (cnpjTermo.Length > 0 && l.Cnpj.Contains(cnpjTermo)));
        }

        if (ativo.HasValue)     query = query.Where(l => l.StatusLoja == ativo.Value);
        if (auditorId.HasValue) query = query.Where(l => l.AuditorUsuarioId == auditorId.Value);
        if (gerenteId.HasValue) query = query.Where(l => l.GerenteUsuarioId == gerenteId.Value);

        var total = await query.CountAsync(ct);

        var itens = await query
            .OrderBy(l => l.RazaoSocial)
            .ThenBy(l => l.LojaId)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync(ct);

        return (itens, total);
    }
    public async Task<IReadOnlyList<Usuario>> ListarGerentesDisponiveisAsync(
        string? busca, Guid? lojaAtualId, int limite, CancellationToken ct)
    {
        var tipoGerente = (int)TipoUsuarioEnum.Gerente;

        IQueryable<Usuario> query = _db.Usuarios
            .AsNoTracking()
            .Where(u => u.TipoUsuarioId == tipoGerente && u.StatusUsuario)
            .Where(u => !_db.Lojas.Any(l =>
                l.GerenteUsuarioId == u.UsuarioId
                && (lojaAtualId == null || l.LojaId != lojaAtualId.Value)));

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            query = query.Where(u => u.NomeUsuario.Contains(termo));
        }

        return await query
            .OrderBy(u => u.NomeUsuario)
            .ThenBy(u => u.UsuarioId)
            .Take(limite)
            .ToListAsync(ct);
    }
    
    public Task<bool> UsuarioVinculadoAsync(Guid usuarioId, CancellationToken ct) =>
        _db.Lojas.AnyAsync(l => l.StatusLoja &&
                                (l.AuditorUsuarioId == usuarioId || l.GerenteUsuarioId == usuarioId), ct);
    
    public void Adicionar(Loja loja) => _db.Lojas.Add(loja);

    public async Task SalvarAsync(CancellationToken ct)
    {
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new DomainException.ConflitoException("Já existe uma loja com esses dados.");
        }
    }
}
