using OsLog.Domain.Enums.Empresa;

namespace OsLog.Domain.Entities;

public class UsuarioAcesso
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    /// <summary>
    /// Id do usuário no ASP.NET Identity.
    /// Esse valor corresponde ao ApplicationUser.Id.
    /// </summary>
    public string IdentityUserId { get; set; } = null!;
    public int EmpresaId { get; set; }
    /// <summary>
    /// null = acesso à todas as filiais da empresa
    /// </summary>
    public int? UnidadeId { get; set; }
    public PerfilAcessoEmpresa Perfil { get; set; }

    public bool FlAtivo { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataAlteracao { get; set; }
    public int? AlteradoPor { get; set; }

    // Navegações
    public Empresa Empresa { get; set; } = null!;
    public Unidade? Unidade { get; set; }
}
