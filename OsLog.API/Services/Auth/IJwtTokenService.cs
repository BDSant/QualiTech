using System.Security.Claims;
using OsLog.Api.DTOs.Auth;

namespace OsLog.API.Services.Auth;

public interface IJwtTokenService
{
    /// <summary>
    /// Gera AccessToken + RefreshToken para um usuário.
    /// </summary>
    /// <param name="userId">Id do usuário (AspNetUsers.Id)</param>
    /// <param name="email">Email do usuário</param>
    /// <param name="userName">UserName (login / nome de exibição)</param>
    /// <param name="roles">Lista de roles do Identity (Admin, Tecnico, etc.)</param>
    /// <param name="additionalClaims">Outras claims que você queira acrescentar</param>
    Task<TokenResponseDto> GenerateTokensAsync(
        string userId,
        string? email,
        string? userName,
        IEnumerable<string> roles,
        IEnumerable<Claim>? additionalClaims = null,
        CancellationToken ct = default);
}
