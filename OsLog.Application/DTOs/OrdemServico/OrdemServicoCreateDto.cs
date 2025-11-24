namespace OsLog.Application.DTOs.OrdemServico;

public class OrdemServicoCreateDto
{
    public int EmpresaId { get; set; }
    public int UnidadeId { get; set; }
    public int ClienteId { get; set; }
    public int? TecnicoId { get; set; }
    public string DescricaoProblema { get; set; } = string.Empty;
    public bool SinalObrigatorio { get; set; }
    public decimal? ValorSinal { get; set; }

    public List<OrdemServicoAcessorioDto> Acessorios { get; set; } = new();
    public List<OrcamentoItemDto> Itens { get; set; } = new();
}
