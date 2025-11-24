namespace OsLog.Domain.Entities;

public class Tecnico
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int UnidadeId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Apelido { get; set; }
    public bool Ativo { get; set; } = true;
    public bool FlExcluido { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataAlteracao { get; set; }
    public int? AlteradoPor { get; set; }

    public ICollection<OrdemServico> OrdensServico { get; set; } = new List<OrdemServico>();
    public ICollection<OrdemServicoComissao> Comissoes { get; set; } = new List<OrdemServicoComissao>();
}
