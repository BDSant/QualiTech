using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OsLog.Domain.Entities;

namespace OsLog.Infrastructure.EntityFramework.Configurations;

public class OrdemServicoConfiguration : IEntityTypeConfiguration<OrdemServico>
{
    public void Configure(EntityTypeBuilder<OrdemServico> builder)
    {
        builder.ToTable("OrdemServico", "OrdemServico");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.StatusOs)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.StatusOrcamento)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.ValorSinal)
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.ValorOrcamentoTotal)
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.DataCriacao)
            .IsRequired();

        builder.Property(o => o.DescricaoProblema)
            .HasColumnType("varchar(max)")
            .IsRequired();


        builder.HasOne(o => o.Cliente)
            .WithMany(c => c.OrdensServico)
            .HasForeignKey(o => o.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Tecnico)
            .WithMany(t => t.OrdensServico)
            .HasForeignKey(o => o.TecnicoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(o => o.DescricaoProblema)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasOne(o => o.Empresa)
            .WithMany()
            .HasForeignKey(o => o.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Unidade)
            .WithMany(u => u.OrdensServico)
            .HasForeignKey(o => o.UnidadeId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}
