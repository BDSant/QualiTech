using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OsLog.Domain.Entities;

namespace OsLog.Infrastructure.EntityFramework.Configurations;

public class PagamentoOsConfiguration : IEntityTypeConfiguration<PagamentoOs>
{
    public void Configure(EntityTypeBuilder<PagamentoOs> builder)
    {
        builder.ToTable("PagamentoOs", "OrdemServico");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.TipoPagamento)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.FormaPagamento)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Valor)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.StatusRegistro)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.DataRegistro)
            .IsRequired();

        builder.HasOne(p => p.OrdemServico)
            .WithMany(os => os.Pagamentos)
            .HasForeignKey(p => p.OrdemServicoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
