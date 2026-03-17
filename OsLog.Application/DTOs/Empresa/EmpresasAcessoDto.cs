using OsLog.Domain.Enums;

namespace OsLog.Application.DTOs.Empresa;

public class UnidadeAcessoDto
{
    public int UnidadeId { get; set; }
    public string NomeUnidade { get; set; } = null!;
    public PerfilAcesso Perfil { get; set; }
}

public class EmpresaAcessoDto
{
    public Guid? EmpresaId { get; set; }
    public string RazaoSocial { get; set; } = null!;
    public string NomeFantasia { get; set; } = null!;

    /// <summary>
    /// true = usuário tem um vínculo com UnidadeId = null (acesso a TODAS as unidades dessa empresa)
    /// </summary>
    public bool AcessoTotalEmpresa { get; set; }

    /// <summary>
    /// Unidades às quais o usuário está explicitamente vinculado
    /// (para quem não tem acesso total, é a lista efetiva de unidades).
    /// </summary>
    public IReadOnlyCollection<UnidadeAcessoDto> Unidades { get; set; }
        = Array.Empty<UnidadeAcessoDto>();
}
