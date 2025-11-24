using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OsLog.Domain.Entities;

public class OrdemServicoFotoConfiguration : IEntityTypeConfiguration<OrdemServicoFoto>
{
    public void Configure(EntityTypeBuilder<OrdemServicoFoto> builder)
    {
        builder.ToTable("OrdemServicoFoto");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .ValueGeneratedOnAdd();

        // FK para OrdemServico
        builder.Property(f => f.OrdemServicoId)
            .IsRequired();

        builder.HasOne(f => f.OrdemServico)
            .WithMany(os => os.Fotos)
            .HasForeignKey(f => f.OrdemServicoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Descrição (opcional)
        builder.Property(f => f.Descricao)
            .HasMaxLength(255);

        // Conteúdo da foto (VARBINARY(MAX))
        builder.Property(f => f.Foto)
            .HasColumnType("VARBINARY(MAX)")
            .IsRequired(false);

        // DataCadastro
        builder.Property(f => f.DataCadastro)
            .HasDefaultValueSql("GETUTCDATE()");

        // Soft delete
        builder.Property(f => f.FlExcluido)
            .HasDefaultValue(false);

        // Auditoria
        builder.Property(f => f.DataAlteracao);
        builder.Property(f => f.AlteradoPor);

        // Índice útil para buscas
        builder.HasIndex(f => new { f.OrdemServicoId, f.FlExcluido });
    }
}
