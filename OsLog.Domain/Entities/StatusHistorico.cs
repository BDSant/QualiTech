namespace OsLog.Domain.Entities;

public class StatusHistorico
{
    public int Id { get; set; }
    public int OrdemServicoId { get; set; }
    public string TipoEvento { get; set; } = string.Empty;
    public string? StatusOsAnterior { get; set; }
    public string? StatusOsNovo { get; set; }
    public string? DescricaoEvento { get; set; }
    public DateTime DataEvento { get; set; } = DateTime.UtcNow;
    public int? UsuarioId { get; set; }

    public OrdemServico OrdemServico { get; set; } = null!;
}