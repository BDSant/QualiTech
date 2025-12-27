namespace OsLog.Application.Common.Security.Claims;

/// <summary>
/// Centraliza os nomes das claims usadas pelo OsLog.
/// Evita "strings mágicas" espalhadas pelo código.
/// </summary>
public static class OsLogClaimTypes
{
    public const string EmpresaId = "empresa_id";
    public const string UnidadeId = "unidade_id";
    public const string UsuarioId = "usuario_id";
    public const string Perfil = "perfil"; // ex.: Master/Admin/Tecnico
}
