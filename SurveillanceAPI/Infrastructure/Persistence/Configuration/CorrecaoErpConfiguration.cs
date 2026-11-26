using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SurveillanceAPI.Domains;

namespace SurveillanceAPI.Infraestructure.Persistence.Configuration;

public class CorrecaoErpConfiguration : IEntityTypeConfiguration<CorrecaoErp>
{
    public void Configure(EntityTypeBuilder<CorrecaoErp> builder)
    {
        builder.ToTable("CorrecaoErp");
        builder.HasKey(c => c.CorrecaoErpId);
        builder.Property(c => c.CorrecaoErpId).ValueGeneratedNever();

        builder.Property(c => c.IdLancamento).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Ean).HasMaxLength(20);
        builder.Property(c => c.AlteracoesJson).IsRequired();
        builder.Property(c => c.Explicacao).IsRequired().HasMaxLength(4000);
        builder.Property(c => c.HashErp).IsRequired().HasMaxLength(64);
        builder.Property(c => c.HashErpDepois).HasMaxLength(64);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(c => c.CriadaEm).IsRequired();
        builder.Property(c => c.ExpiraEm).IsRequired();

        // Duas aplicações simultâneas da mesma proposta: a segunda falha por concorrência.
        builder.Property(c => c.RowVersion).IsRowVersion();

        builder.HasOne(c => c.Comparacao)
            .WithMany()
            .HasForeignKey(c => c.ComparacaoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Loja>()
            .WithMany()
            .HasForeignKey(c => c.LojaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(c => c.AplicadaPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.ComparacaoId);
        builder.HasIndex(c => new { c.UsuarioId, c.Status });
    }
}
