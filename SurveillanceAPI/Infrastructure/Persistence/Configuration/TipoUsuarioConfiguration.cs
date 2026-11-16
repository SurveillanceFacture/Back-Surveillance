using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SurveillanceAPI.Domains;

namespace SurveillanceAPI.Infraestructure.Persistence.Configuration;

public class TipoUsuarioConfiguration : IEntityTypeConfiguration<TipoUsuario>
{
    public void Configure(EntityTypeBuilder<TipoUsuario> builder)
    {
        builder.ToTable("TipoUsuario");
        builder.HasKey(t => t.TipoUsuarioId);
        builder.Property(t => t.TipoUsuarioId).ValueGeneratedNever();

        builder.Property(t => t.NomeTipoUsuario).IsRequired().HasMaxLength(50);
        builder.HasIndex(t => t.NomeTipoUsuario).IsUnique();

        builder.HasData(
            Enum.GetValues<TipoUsuarioEnum>().Select(e => new
            {
                TipoUsuarioId = (int)e,
                NomeTipoUsuario = e.ToString()
            }));
    }
}