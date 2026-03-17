using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OsLog.Domain.Entities;

namespace OsLog.Infrastructure.EntityFramework.Configurations;

public class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("Empresa", "Empresa");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.RazaoSocial)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.NomeFantasia)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(e => e.Ativa)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.DataCriacaoUtc)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.HasIndex(e => e.RazaoSocial);
        builder.HasIndex(e => e.NomeFantasia);
    }
}