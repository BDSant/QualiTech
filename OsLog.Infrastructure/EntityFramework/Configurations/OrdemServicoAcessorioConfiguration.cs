using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OsLog.Domain.Entities;

namespace OsLog.Infrastructure.EntityFramework.Configurations;

public class OrdemServicoAcessorioConfiguration : IEntityTypeConfiguration<OrdemServicoAcessorio>
{
    public void Configure(EntityTypeBuilder<OrdemServicoAcessorio> builder)
    {
        builder.ToTable("Acessorio", "OrdemServico");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Descricao).IsRequired().HasMaxLength(150);
        builder.Property(a => a.DataCriacao).IsRequired();
        builder.Property(a => a.FlExcluido).HasDefaultValue(false);
        builder.HasOne(a => a.OrdemServico).WithMany(os => os.Acessorios).HasForeignKey(a => a.OrdemServicoId).OnDelete(DeleteBehavior.Cascade);
    }
}
