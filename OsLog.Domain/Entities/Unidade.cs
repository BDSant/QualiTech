namespace OsLog.Domain.Entities;

public class Unidade
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }

    public string Nome { get; set; } = string.Empty;
    public string? Cnpj { get; set; }
    public string? InscricaoEstadual { get; set; }
    public string? InscricaoMunicipal { get; set; }
    public string? Endereco { get; set; }
    public string? Telefone { get; set; }

    // Soft delete
    public bool FlExcluido { get; set; }

    // Auditoria
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataAlteracao { get; set; }
    public int? AlteradoPor { get; set; }

    // Navegações
    public Empresa Empresa { get; set; } = null!;
    public ICollection<UsuarioAcesso> UsuariosAcesso { get; set; } = new List<UsuarioAcesso>();
}
