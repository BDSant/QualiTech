using Microsoft.IdentityModel.Tokens;
using OsLog.Application.Common.Result;
using OsLog.Application.Common.Security.ErrorCodes;
using OsLog.Application.Ports.Security;
using OsLog.Application.UseCases.Autenticacao.Common;

namespace OsLog.Application.UseCases.Autenticacao.RefreshToken;

public sealed class RefreshTokenUseCase : IRefreshTokenUseCase
{
    private readonly IJwtTokenService _jwt;

    public RefreshTokenUseCase(IJwtTokenService jwt) => _jwt = jwt;

    public async Task<Result<TokenResponse>> ExecuteAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Result<TokenResponse>.Fail(new AppError(AuthErrorCodes.RefreshTokenRequired, "RefreshToken é obrigatório.", ErrorType.Validation, "refreshToken"));

        try
        {
            var token = await _jwt.RefreshAsync(request.RefreshToken, ct);
            return Result<TokenResponse>.Ok(token);
        }
        catch (SecurityTokenException)
        {
            return Result<TokenResponse>.Fail(new AppError(AuthErrorCodes.RefreshInvalid, "Refresh token inválido ou expirado.", ErrorType.Unauthorized));
        }
    }
}
