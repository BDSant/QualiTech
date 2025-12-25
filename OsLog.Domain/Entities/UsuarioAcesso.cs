using OsLog.Domain.Enums.Empresa;

namespace OsLog.Domain.Entities;

public class UsuarioAcesso
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!; // FK para AspNetUsers.Id
    public int EmpresaId { get; set; }
    public int? UnidadeId { get; set; } // null = acesso à empresa toda
    public PerfilAcessoEmpresa Perfil { get; set; }

    public bool FlExcluido { get; set; }
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    public DateTime? DataAlteracao { get; set; }
    public int? AlteradoPor { get; set; }

    // Navegações
    public Empresa Empresa { get; set; } = null!;
    public Unidade? Unidade { get; set; }
}
