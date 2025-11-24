using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OsLog.Domain.Entities;

namespace OsLog.Infrastructure.EF.Configurations;

public class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("Empresa", "Empresa");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.RazaoSocial)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.NomeFantasia)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Cnpj)
            .HasMaxLength(18);

        builder.Property(e => e.FlExcluido)
            .HasDefaultValue(false);

        builder.Property(e => e.DataCriacao)
            .IsRequired();
    }
}
