using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OsLog.Domain.Entities;

namespace OsLog.Infrastructure.EntityFramework.Configurations.EmpresaConfig;

public class UsuarioAcessoConfiguration : IEntityTypeConfiguration<UsuarioAcesso>
{
    public void Configure(EntityTypeBuilder<UsuarioAcesso> builder)
    {
        builder.ToTable("UsuarioAcesso", "Empresa");

        builder.HasKey(ua => ua.Id);

        builder.Property(x => x.Nome)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(ua => ua.IdentityUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(ua => ua.Perfil)
            .IsRequired();

        builder.Property(ua => ua.FlAtivo)
            .HasDefaultValue(true);

        builder.Property(ua => ua.DataCriacao)
            .IsRequired();

        builder.HasOne(ua => ua.Empresa)
            .WithMany(e => e.UsuariosAcesso)
            .HasForeignKey(ua => ua.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ua => ua.Unidade)
            .WithMany(u => u.UsuariosAcesso)
            .HasForeignKey(ua => ua.UnidadeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(ua => ua.EmpresaId);

        builder.HasIndex(ua => ua.UnidadeId);

        builder.HasIndex(ua => new { ua.IdentityUserId, ua.EmpresaId })
            .HasDatabaseName("IX_UsuarioAcesso_IdentityUserId_EmpresaId");
    }
}