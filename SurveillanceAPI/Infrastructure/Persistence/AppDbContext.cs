using Microsoft.EntityFrameworkCore;
using SurveillanceAPI.Domains;

namespace SurveillanceAPI.Infraestructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<TipoUsuario> TiposUsuario => Set<TipoUsuario>();
    public DbSet<Loja> Lojas => Set<Loja>();
    public DbSet<Comparacao> Comparacoes => Set<Comparacao>();
    public DbSet<Divergencia> Divergencias => Set<Divergencia>();
    public DbSet<HistoricoDivergencia> HistoricosDivergencia => Set<HistoricoDivergencia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}