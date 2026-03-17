using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OsLog.Domain.Entities;

namespace OsLog.Infrastructure.EntityFramework.Configurations.EmpresaConfig;

public class UsuarioAcessoConfiguration : IEntityTypeConfiguration<UsuarioAcesso>
{
    public void Configure(EntityTypeBuilder<UsuarioAcesso> builder)
    {
        builder.ToTable("UsuarioAcesso", "Empresa");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UsuarioId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Escopo)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Perfil)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.DataCriacaoUtc)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.HasOne(x => x.Empresa)
            .WithMany(x => x.UsuariosAcesso)
            .HasForeignKey(x => x.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Unidade)
            .WithMany(x => x.UsuariosAcesso)
            .HasForeignKey(x => x.UnidadeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.UsuarioId);
        builder.HasIndex(x => x.EmpresaId);
        builder.HasIndex(x => x.UnidadeId);
        builder.HasIndex(x => new { x.UsuarioId, x.EmpresaId });
    }
}