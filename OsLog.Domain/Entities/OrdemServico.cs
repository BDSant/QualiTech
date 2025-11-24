namespace OsLog.Domain.Entities;

public class OrdemServico
{
    public int Id { get; set; }

    // Multi-tenant / contexto da OS
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public int UnidadeId { get; set; }              // era UnidadeId
    public Unidade Unidade { get; set; } = null!;

    public int ClienteId { get; set; }
    public int? TecnicoId { get; set; }
    
    public string DescricaoProblema { get; set; } = string.Empty;

    // Status / fluxo
    public string StatusOs { get; set; } = "ABERTA";
    public string StatusOrcamento { get; set; } = "PENDENTE";

    // Sinal
    public bool SinalObrigatorio { get; set; }
    public bool SinalPago { get; set; }
    public decimal? ValorSinal { get; set; }
    public decimal? ValorOrcamentoTotal { get; set; }

    // Soft delete
    public bool FlExcluido { get; set; }

    // Auditoria
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataAlteracao { get; set; }
    public int? AlteradoPor { get; set; }

    // Navegações já existentes
    public Cliente Cliente { get; set; } = null!;
    public Tecnico? Tecnico { get; set; }

    public ICollection<OrcamentoItem> Itens { get; set; } = new List<OrcamentoItem>();
    public ICollection<PagamentoOs> Pagamentos { get; set; } = new List<PagamentoOs>();
    public ICollection<StatusHistorico> Historicos { get; set; } = new List<StatusHistorico>();
    public ICollection<OrdemServicoAcessorio> Acessorios { get; set; } = new List<OrdemServicoAcessorio>();
    public ICollection<OrdemServicoFoto> Fotos { get; set; } = new List<OrdemServicoFoto>();
    public ICollection<OrdemServicoComissao> Comissoes { get; set; } = new List<OrdemServicoComissao>();
}
