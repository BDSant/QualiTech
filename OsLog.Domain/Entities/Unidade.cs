using OsLog.Domain.Enums;

namespace OsLog.Domain.Entities;

public class Unidade
{
    public int Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Cnpj { get; set; } = string.Empty;

    public string? InscricaoEstadual { get; set; }

    public string? InscricaoMunicipal { get; set; }

    public string? Endereco { get; set; }

    public string? Telefone { get; set; }

    public TipoUnidade Tipo { get; set; }

    public bool Ativa { get; set; } = true;

    public DateTime DataCriacaoUtc { get; set; } = DateTime.UtcNow;

    public Empresa Empresa { get; set; } = null!;

    public ICollection<UsuarioAcesso> UsuariosAcesso { get; set; } = new List<UsuarioAcesso>();
}