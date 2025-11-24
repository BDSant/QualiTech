using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OsLog.Domain.Entities;

namespace OsLog.Infrastructure.EntityFramework.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Cliente", "Clientes");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Nome).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Documento).HasMaxLength(20);
        builder.Property(c => c.Telefone).HasMaxLength(20);
        builder.Property(c => c.Email).HasMaxLength(150);
        builder.Property(c => c.DataCriacao).IsRequired();
    }
}
