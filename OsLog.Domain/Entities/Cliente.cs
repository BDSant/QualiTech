namespace OsLog.Domain.Entities;

public class Cliente
{
    public int Id { get; set; }                      // PK

    public int EmpresaId { get; set; }
    public int UnidadeId { get; set; }

    public string Nome { get; set; } = string.Empty;
    public string? Documento { get; set; }           // CPF/CNPJ se quiser
    public string? Telefone { get; set; }
    public string? Email { get; set; }

    public bool FlExcluido { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataAlteracao { get; set; }
    public int? AlteradoPor { get; set; }

    // Navegação
    //public ICollection<OrdemServico> OrdensServico { get; set; } = new List<OrdemServico>();
}
