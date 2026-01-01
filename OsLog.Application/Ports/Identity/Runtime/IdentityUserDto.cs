namespace OsLog.Application.Ports.Identity.Runtime;

/// <summary>
/// Modelo mínimo de usuário para uso na camada Application.
/// Evita vazar tipos do ASP.NET Identity para fora da Infraestrutura.
/// </summary>
public sealed record IdentityUserDto(
    string Id,
    string Email
    //bool IsActive,
    //bool IsLockedOut,
    //bool IsActiveDirectory
);
