using System.Security.Claims;
using OsLog.Application.Common.Result;

namespace OsLog.Application.Ports.Identity.Runtime;

/// <summary>
/// Gateway para operações de credencial/usuário (Identity, AD, etc.).
/// Implementação concreta reside na Infraestrutura.
/// </summary>
public interface IIdentityGateway
{
    Task<IdentityUserDto?> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<IdentityUserDto?> FindByIdAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// Verifica credenciais. Deve respeitar lockout/contagem de falhas conforme configuração.
    /// </summary>
    Task<bool> CheckPasswordAsync(string userId, string password, CancellationToken ct = default);

    Task<IReadOnlyList<string>> GetRolesAsync(string userId, CancellationToken ct = default);
    Task<IReadOnlyList<Claim>> GetClaimsAsync(string userId, CancellationToken ct = default);

    Task<Result> ChangePasswordAsync(string email, string currentPassword, string newPassword, CancellationToken ct = default);
    Task<Result> ResetPasswordAsync(string email, string newPassword, CancellationToken ct = default);
}
