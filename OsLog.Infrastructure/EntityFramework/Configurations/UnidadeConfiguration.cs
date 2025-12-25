using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OsLog.Domain.Entities;

namespace OsLog.Infrastructure.EntityFramework.Configurations.EmpresaConfig;

public class UnidadeConfiguration : IEntityTypeConfiguration<Unidade>
{
    public void Configure(EntityTypeBuilder<Unidade> builder)
    {
        builder.ToTable("Unidade", "Empresa");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nome)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(u => u.Cnpj)
            .HasMaxLength(18);

        builder.Property(u => u.InscricaoEstadual)
            .HasMaxLength(50);

        builder.Property(u => u.InscricaoMunicipal)
            .HasMaxLength(50);

        builder.Property(u => u.Endereco)
            .HasMaxLength(300);

        builder.Property(u => u.Telefone)
            .HasMaxLength(50);

        builder.Property(u => u.FlExcluido)
            .HasDefaultValue(false);

        builder.Property(u => u.DataCriacao)
            .IsRequired();

        builder.HasOne(u => u.Empresa)
            .WithMany(e => e.Unidades)
            .HasForeignKey(u => u.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
