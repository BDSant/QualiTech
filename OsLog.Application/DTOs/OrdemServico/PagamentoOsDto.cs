namespace OsLog.Application.DTOs.OrdemServico;

public class PagamentoOsDto
{
    public int Id { get; set; }
    public string TipoPagamento { get; set; } = string.Empty;
    public string FormaPagamento { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string StatusRegistro { get; set; } = string.Empty;
    public DateTime DataRegistro { get; set; }
    public DateTime? DataConfirmacao { get; set; }
}
