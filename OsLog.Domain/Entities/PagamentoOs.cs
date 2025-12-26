using OsLog.Domain.Enums;

namespace OsLog.Domain.Entities;

public class PagamentoOs
{
    public int Id { get; set; }
    public int OrdemServicoId { get; set; }
    public TipoPagamento TipoPagamento { get; set; } = TipoPagamento.NaoDefinido;
    public FormaPagamento FormaPagamento { get; set; } = FormaPagamento.NaoDefinido;
    public decimal Valor { get; set; }
    public StatusRegistro StatusRegistro { get; set; } = StatusRegistro.Pendente;
    public DateTime DataRegistro { get; set; } = DateTime.UtcNow;
    public DateTime? DataConfirmacao { get; set; }
    public bool FlExcluido { get; set; }

    //// Navegação
    //public OrdemServico OrdemServico { get; set; } = null!;
}
