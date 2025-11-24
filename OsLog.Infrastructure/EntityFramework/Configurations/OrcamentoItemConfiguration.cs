using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OsLog.Domain.Entities;

namespace OsLog.Infrastructure.EntityFramework.Configurations;

public class OrcamentoItemConfiguration : IEntityTypeConfiguration<OrcamentoItem>
{
    public void Configure(EntityTypeBuilder<OrcamentoItem> builder)
    {
        builder.ToTable("OrcamentoItem", "OrdemServico");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.TipoItem)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.DescricaoItem)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Quantidade)
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.ValorUnitario)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(o => o.OrdemServico)
            .WithMany(os => os.Itens)
            .HasForeignKey(o => o.OrdemServicoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
