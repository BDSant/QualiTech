namespace OsLog.Domain.Entities;

public class PagamentoOs
{
    public int Id { get; set; }
    public int OrdemServicoId { get; set; }
    public string TipoPagamento { get; set; } = string.Empty;    // SINAL / RESTANTE / TOTAL
    public string FormaPagamento { get; set; } = string.Empty;   // CARTAO / PIX / DINHEIRO etc.
    public decimal Valor { get; set; }
    public string StatusRegistro { get; set; } = "PENDENTE";     // PENDENTE / CONFIRMADO etc.
    public DateTime DataRegistro { get; set; } = DateTime.UtcNow;
    public DateTime? DataConfirmacao { get; set; }
    public bool FlExcluido { get; set; }

    //// Navegação
    //public OrdemServico OrdemServico { get; set; } = null!;
}
