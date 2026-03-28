using System.Security.Claims;
using OsLog.Application.UseCases.Autenticacao.Common;

namespace OsLog.Application.Ports.Security;

public interface IJwtTokenService
{
    Task<TokenResponse> GenerateTokensAsync(
        string userId,
        string email,
        IEnumerable<string> roles,
        IEnumerable<Claim>? additionalClaims = null,
        CancellationToken ct = default);

    Task<TokenResponse> GenerateTokensAsync(
        string userId,
        string email,
        IEnumerable<string> roles,
        int usuarioId,
        IEnumerable<Claim>? additionalClaims = null,
        CancellationToken ct = default);

    Task<TokenResponse> RefreshAsync(
        string refreshToken,
        CancellationToken ct = default);

    Task LogoutAsync(
        string refreshToken,
        CancellationToken ct = default);
}