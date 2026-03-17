using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OsLog.Domain.Entities;
using OsLog.Domain.Enums;

namespace OsLog.Infrastructure.EntityFramework.Configurations.EmpresaConfig;

public class UnidadeConfiguration : IEntityTypeConfiguration<Unidade>
{
    public void Configure(EntityTypeBuilder<Unidade> builder)
    {
        builder.ToTable("Unidade", "Empresa");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Cnpj)
            .IsRequired()
            .HasMaxLength(18);

        builder.Property(u => u.InscricaoEstadual)
            .HasMaxLength(50);

        builder.Property(u => u.InscricaoMunicipal)
            .HasMaxLength(50);

        builder.Property(u => u.Endereco)
            .HasMaxLength(300);

        builder.Property(u => u.Telefone)
            .HasMaxLength(50);

        builder.Property(u => u.Tipo)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(u => u.Ativa)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.DataCriacaoUtc)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.HasOne(u => u.Empresa)
            .WithMany(e => e.Unidades)
            .HasForeignKey(u => u.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(u => u.EmpresaId);
        builder.HasIndex(u => u.Cnpj).IsUnique();
        builder.HasIndex(u => new { u.EmpresaId, u.Tipo });
    }
}