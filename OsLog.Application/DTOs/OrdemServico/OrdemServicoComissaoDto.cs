namespace OsLog.Application.DTOs.OrdemServico;

public class OrdemServicoComissaoDto
{
    public int Id { get; set; }
    public int TecnicoId { get; set; }
    public string TipoBase { get; set; } = string.Empty;
    public decimal Percentual { get; set; }
    public decimal ValorBase { get; set; }
    public decimal ValorComissao { get; set; }
    public bool Liquidado { get; set; }
    public DateTime DataGeracao { get; set; }
    public DateTime? DataLiquidacao { get; set; }
}
