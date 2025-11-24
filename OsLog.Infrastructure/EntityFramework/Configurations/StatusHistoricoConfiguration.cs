using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OsLog.Domain.Entities;

namespace OsLog.Infrastructure.EntityFramework.Configurations;

public class StatusHistoricoConfiguration : IEntityTypeConfiguration<StatusHistorico>
{
    public void Configure(EntityTypeBuilder<StatusHistorico> builder)
    {
        builder.ToTable("StatusHistorico", "OrdemServico");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.TipoEvento)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.StatusOsAnterior)
            .HasMaxLength(50);

        builder.Property(s => s.StatusOsNovo)
            .HasMaxLength(50);

        builder.Property(s => s.DataEvento)
            .IsRequired();

        builder.HasOne(s => s.OrdemServico)
            .WithMany(os => os.Historicos)
            .HasForeignKey(s => s.OrdemServicoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
