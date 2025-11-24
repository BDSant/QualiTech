using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OsLog.Domain.Entities;

namespace OsLog.Infrastructure.EntityFramework.Configurations;

public class OrdemServicoComissaoConfiguration : IEntityTypeConfiguration<OrdemServicoComissao>
{
    public void Configure(EntityTypeBuilder<OrdemServicoComissao> builder)
    {
        builder.ToTable("ComissaoTecnico", "OrdemServico");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.TipoBase)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Percentual)
            .HasColumnType("decimal(5,2)");      // até 999.99% (mais do que suficiente)

        builder.Property(c => c.ValorBase)
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.ValorComissao)
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.DataGeracao)
            .IsRequired();

        builder.Property(c => c.FlExcluido)
            .HasDefaultValue(false);

        builder.HasOne(c => c.OrdemServico)
            .WithMany(os => os.Comissoes)
            .HasForeignKey(c => c.OrdemServicoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Tecnico)
            .WithMany(t => t.Comissoes)
            .HasForeignKey(c => c.TecnicoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
