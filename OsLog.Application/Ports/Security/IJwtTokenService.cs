using System.Security.Claims;
using OsLog.Application.DTOs.Auth;

namespace OsLog.Application.Ports.Security;

public interface IJwtTokenService
{
    Task<TokenResponseDto> GenerateTokensAsync(
        string userId,
        string email,
        IEnumerable<string> roles,
        IEnumerable<Claim>? additionalClaims = null,
        CancellationToken ct = default);

    Task<TokenResponseDto> GenerateTokensAsync(
        string userId,
        string email,
        IEnumerable<string> roles,
        int usuarioId,
        IEnumerable<Claim>? additionalClaims = null,
        CancellationToken ct = default);

    Task<TokenResponseDto> RefreshAsync(
        string refreshToken,
        CancellationToken ct = default);

    Task LogoutAsync(
        string refreshToken,
        CancellationToken ct = default);
}