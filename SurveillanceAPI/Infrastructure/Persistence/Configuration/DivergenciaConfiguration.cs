using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SurveillanceAPI.Domains;

namespace SurveillanceAPI.Infraestructure.Persistence.Configuration;

public class DivergenciaConfiguration : IEntityTypeConfiguration<Divergencia>
{
    public void Configure(EntityTypeBuilder<Divergencia> builder)
    {
        builder.ToTable("Divergencia");
        builder.HasKey(d => d.DivergenciaId);

        builder.Property(d => d.Campo).IsRequired().HasMaxLength(200);
        builder.Property(d => d.ValorXml).HasMaxLength(500);
        builder.Property(d => d.ValorJson).HasMaxLength(500);
        builder.Property(d => d.Observacao).HasMaxLength(1000);
        builder.Property(d => d.DataDeteccao).IsRequired();

        builder.Property(d => d.Gravidade).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasOne(d => d.ResolvidoPorUsuario)
            .WithMany()
            .HasForeignKey(d => d.ResolvidoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // Divergencia -> Historico: configurada só aqui
        builder.HasMany(d => d.Historico)
            .WithOne(h => h.Divergencia)
            .HasForeignKey(h => h.DivergenciaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(d => d.Historico).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(d => new { d.ComparacaoId, d.Campo });
        builder.HasIndex(d => d.Status);
    }
}