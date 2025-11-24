namespace OsLog.Application.DTOs.OrdemServico;

public class OrcamentoItemDto
{
    public int Id { get; set; }
    public string TipoItem { get; set; } = string.Empty;
    public string DescricaoItem { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
}
