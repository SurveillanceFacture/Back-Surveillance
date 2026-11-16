using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SurveillanceAPI.Domains;

namespace SurveillanceAPI.Infraestructure.Persistence.Configuration;

public class LojaConfiguration : IEntityTypeConfiguration<Loja>
{
    public void Configure(EntityTypeBuilder<Loja> builder)
    {
        builder.ToTable("Loja");
        builder.HasKey(l => l.LojaId);

        builder.Property(l => l.RazaoSocial).IsRequired().HasMaxLength(180);
        builder.Property(l => l.Cnpj).IsRequired().HasMaxLength(14);
        builder.HasIndex(l => l.Cnpj).IsUnique();
        builder.Property(l => l.DataCriacao).IsRequired();

        // 1 auditor -> N lojas
        builder.HasOne(l => l.Auditor)
            .WithMany()
            .HasForeignKey(l => l.AuditorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // 1 gerente <-> 1 loja (índice único criado automaticamente)
        builder.HasOne(l => l.Gerente)
            .WithOne()
            .HasForeignKey<Loja>(l => l.GerenteUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // 1 usuário -> N lojas criadas
        builder.HasOne(l => l.CriadoPorUsuario)
            .WithMany()
            .HasForeignKey(l => l.CriadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
