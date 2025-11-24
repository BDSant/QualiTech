namespace OsLog.Application.DTOs.OrdemServico;

public class StatusHistoricoDto
{
    public int Id { get; set; }
    public string TipoEvento { get; set; } = string.Empty;
    public string? StatusOsAnterior { get; set; }
    public string? StatusOsNovo { get; set; }
    public string? DescricaoEvento { get; set; }
    public DateTime DataEvento { get; set; }
    public int? UsuarioId { get; set; }
}
