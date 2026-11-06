using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SurveillanceAPI.Domains;

namespace SurveillanceAPI.Infraestructure.Persistence.Configuration;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuario");
        builder.HasKey(u => u.UsuarioId);

        builder.Property(u => u.NomeUsuario).IsRequired().HasMaxLength(150);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(254);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.HashSenha).IsRequired();
        builder.Property(u => u.DataCriacao).IsRequired();

        builder.HasOne(u => u.TipoUsuario)
            .WithMany()
            .HasForeignKey(u => u.TipoUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}