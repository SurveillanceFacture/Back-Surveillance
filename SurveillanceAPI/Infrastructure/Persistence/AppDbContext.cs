using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
    protected override void ConfigureConventions(ModelConfigurationBuilder cb)
    {
        cb.Properties<DateTime>().HaveConversion<UtcConverter>();
        cb.Properties<DateTime?>().HaveConversion<UtcNullableConverter>();
    }

    public sealed class UtcConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcConverter() : base(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc)) { }
    }

    public sealed class UtcNullableConverter : ValueConverter<DateTime?, DateTime?>
    {
        public UtcNullableConverter() : base(
            v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v) { }
    }
}