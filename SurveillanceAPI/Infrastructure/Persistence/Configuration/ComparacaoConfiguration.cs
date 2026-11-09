using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SurveillanceAPI.Domains;

namespace SurveillanceAPI.Infraestructure.Persistence.Configuration;

public class ComparacaoConfiguration : IEntityTypeConfiguration<Comparacao>
{
    public void Configure(EntityTypeBuilder<Comparacao> builder)
    {
        builder.ToTable("Comparacao");
        builder.HasKey(c => c.ComparacaoId);

        builder.Property(c => c.ChaveDocumento).IsRequired().HasMaxLength(44);
        builder.Property(c => c.HashXml).IsRequired().HasMaxLength(64);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(c => c.DataComparacao).IsRequired();

        builder.HasOne(c => c.Loja)
            .WithMany()
            .HasForeignKey(c => c.LojaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Auditor)
            .WithMany()
            .HasForeignKey(c => c.AuditorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // Comparacao -> Divergencia: configurada só aqui
        builder.HasMany(c => c.Divergencias)
            .WithOne(d => d.Comparacao)
            .HasForeignKey(d => d.ComparacaoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(c => c.Divergencias).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(c => new { c.LojaId, c.ChaveDocumento });
        builder.HasIndex(c => c.DataComparacao);
    }
}