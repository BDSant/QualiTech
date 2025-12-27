using System.Security.Claims;
using OsLog.Application.DTOs.Auth;

namespace OsLog.Application.Abstractions.Security;

/// <summary>
/// Abstração de emissão/rotação de tokens (Access + Refresh).
/// Implementação concreta reside na Infraestrutura.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Gera AccessToken + RefreshToken para um usuário.
    /// </summary>
    Task<TokenResponseDto> GenerateTokensAsync(
        string userId,
        string email,
        IEnumerable<string> roles,
        IEnumerable<Claim>? additionalClaims = null,
        CancellationToken ct = default);

    Task<TokenResponseDto> RefreshAsync(string refreshToken, CancellationToken ct = default);

    Task LogoutAsync(string refreshToken, CancellationToken ct = default);
}
