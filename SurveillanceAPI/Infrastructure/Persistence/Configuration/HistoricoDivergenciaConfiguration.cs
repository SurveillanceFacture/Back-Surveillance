using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SurveillanceAPI.Domains;

namespace SurveillanceAPI.Infraestructure.Persistence.Configuration;

public class HistoricoDivergenciaConfiguration : IEntityTypeConfiguration<HistoricoDivergencia>
{
    public void Configure(EntityTypeBuilder<HistoricoDivergencia> builder)
    {
        builder.ToTable("HistoricoDivergencia");
        builder.HasKey(h => h.HistoricoDivergenciaId);
        builder.Property(h => h.HistoricoDivergenciaId).ValueGeneratedOnAdd();
        
        builder.Property(h => h.StatusAnterior).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(h => h.StatusNovo).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(h => h.Observacao).HasMaxLength(1000);
        builder.Property(h => h.DataAlteracao).IsRequired();

        builder.HasOne(h => h.Usuario)
            .WithMany()
            .HasForeignKey(h => h.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(h => h.DivergenciaId);
    }
}