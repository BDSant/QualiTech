namespace OsLog.Application.DTOs.OrdemServico;

public class OrdemServicoUpdateDto
{
    public int Id { get; set; }

    public int? TecnicoId { get; set; }

    public string? StatusOs { get; set; }
    public string? StatusOrcamento { get; set; }

    public bool? SinalObrigatorio { get; set; }
    public bool? SinalPago { get; set; }
    public decimal? ValorSinal { get; set; }
    public decimal? ValorOrcamentoTotal { get; set; }
}
