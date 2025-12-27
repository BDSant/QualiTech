using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.Jwt.Core.Interfaces;
using OsLog.Application.Abstractions.Security;
using OsLog.Application.Common.Security.Jwt;
using OsLog.Application.DTOs.Auth;
using OsLog.Application.Interfaces.Services;
using OsLog.Infrastructure.Identity;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace OsLog.Infrastructure.Security.Jwt;

/// <summary>
/// Implementação concreta de emissão/rotação de JWT usando NetDevPack (JWKS).
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly UserManager<ApplicationUser> _userManager;

    public JwtTokenService(
        IOptions<JwtOptions> jwtOptions,
        IJwtService jwtService,
        IRefreshTokenStore refreshTokenStore,
        UserManager<ApplicationUser> userManager)
    {
        _jwtOptions = jwtOptions.Value;
        _jwtService = jwtService;
        _refreshTokenStore = refreshTokenStore;
        _userManager = userManager;
    }

    public Task<TokenResponseDto> GenerateTokensAsync(
        string userId,
        string email,
        IEnumerable<string> roles,
        IEnumerable<Claim>? additionalClaims = null,
        CancellationToken ct = default)
        => GenerateTokensInternalAsync(userId, email, roles, additionalClaims, rotateFromRefreshTokenId: null, ct);

    public async Task<TokenResponseDto> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var oldHash = Sha256Hex(refreshToken);

        var valid = await _refreshTokenStore.GetValidAsync(oldHash, ct);
        if (valid is null)
            throw new SecurityTokenException("Refresh token inválido ou expirado.");

        var (oldId, userId) = valid.Value;

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            throw new SecurityTokenException("Usuário não encontrado.");

        var roles = await _userManager.GetRolesAsync(user);
        var userClaims = await _userManager.GetClaimsAsync(user);

        // Rotaciona: gera novo refresh e revoga o anterior apontando para o novo hash
        return await GenerateTokensInternalAsync(
            user.Id,
            user.Email ?? user.UserName ?? string.Empty,
            roles,
            userClaims,
            rotateFromRefreshTokenId: oldId,
            ct);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = Sha256Hex(refreshToken);
        await _refreshTokenStore.RevokeByHashAsync(hash, ct);
    }

    private async Task<TokenResponseDto> GenerateTokensInternalAsync(
        string userId,
        string email,
        IEnumerable<string> roles,
        IEnumerable<Claim>? additionalClaims,
        Guid? rotateFromRefreshTokenId,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        if (additionalClaims is not null)
        {
            foreach (var c in additionalClaims)
            {
                if (!claims.Any(x => x.Type == c.Type && x.Value == c.Value))
                    claims.Add(c);
            }
        }

        var signingCredentials = await _jwtService.GetCurrentSigningCredentials();

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = now.AddMinutes(_jwtOptions.AccessTokenMinutes),
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = signingCredentials
        };

        var handler = new JsonWebTokenHandler();
        var accessToken = handler.CreateToken(descriptor);

        var refreshToken = GenerateSecureRefreshToken();
        var refreshHash = Sha256Hex(refreshToken);
        var refreshExpires = now.AddDays(_jwtOptions.RefreshTokenDays);

        await _refreshTokenStore.CreateAsync(userId, refreshHash, refreshExpires, ct);

        if (rotateFromRefreshTokenId.HasValue)
            await _refreshTokenStore.RevokeAsync(rotateFromRefreshTokenId.Value, refreshHash, ct);

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            ExpiresAtUtc = descriptor.Expires!.Value.ToUniversalTime(),
            RefreshToken = refreshToken,
            RefreshExpiresAtUtc = refreshExpires
        };
    }

    private static string GenerateSecureRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Base64UrlEncoder.Encode(bytes);
    }

    private static string Sha256Hex(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
