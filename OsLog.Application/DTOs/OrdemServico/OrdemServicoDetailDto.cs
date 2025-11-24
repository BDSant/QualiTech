namespace OsLog.Application.DTOs.OrdemServico;

public class OrdemServicoDetailDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int UnidadeId { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public int? TecnicoId { get; set; }
    public string? TecnicoNome { get; set; }
    public string DescricaoProblema { get; set; } = string.Empty;
    public string StatusOs { get; set; } = string.Empty;
    public string StatusOrcamento { get; set; } = string.Empty;
    public bool SinalObrigatorio { get; set; }
    public bool SinalPago { get; set; }
    public decimal? ValorSinal { get; set; }
    public decimal? ValorOrcamentoTotal { get; set; }
    public bool FlExcluido { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAlteracao { get; set; }

    public List<OrcamentoItemDto> Itens { get; set; } = new();
    public List<PagamentoOsDto> Pagamentos { get; set; } = new();
    public List<StatusHistoricoDto> Historicos { get; set; } = new();
    public List<OrdemServicoAcessorioDto> Acessorios { get; set; } = new();
    public List<OrdemServicoFotoDto> Fotos { get; set; } = new();
    public List<OrdemServicoComissaoDto> Comissoes { get; set; } = new();
}
