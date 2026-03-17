using OsLog.Domain.Enums;

namespace OsLog.Domain.Entities;

public class UsuarioAcesso
{
    public int Id { get; set; }

    public string UsuarioId { get; set; } = string.Empty;

    public Guid? EmpresaId { get; set; }

    public int? UnidadeId { get; set; }

    public EscopoAcesso Escopo { get; set; }

    public PerfilAcesso Perfil { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTime DataCriacaoUtc { get; set; } = DateTime.UtcNow;

    public Empresa? Empresa { get; set; }

    public Unidade? Unidade { get; set; }
}