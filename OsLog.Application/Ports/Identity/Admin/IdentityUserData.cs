namespace OsLog.Application.Ports.Identity.Admin;

/// <summary>
/// Modelo mínimo para representar um usuário de Identity na camada Application,
/// sem depender de ASP.NET Identity diretamente.
/// </summary>
public sealed record IdentityUserData(
    string Id,
    string UserName,
    string Email,
    bool EmailConfirmed
);
