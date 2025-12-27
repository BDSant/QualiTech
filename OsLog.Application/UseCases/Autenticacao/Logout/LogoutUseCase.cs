using OsLog.Application.Abstractions.Security;
using OsLog.Application.Common.Result;
using OsLog.Application.Common.Security.ErrorCodes;

namespace OsLog.Application.UseCases.Autenticacao.Logout;

public sealed class LogoutUseCase : ILogoutUseCase
{
    private readonly IJwtTokenService _jwt;
    public LogoutUseCase(IJwtTokenService jwt) => _jwt = jwt;

    public async Task<Result> ExecuteAsync(LogoutRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Result.Fail(new AppError(AuthErrorCodes.RefreshTokenRequired, "RefreshToken é obrigatório.", ErrorType.Validation, "refreshToken"));

        await _jwt.LogoutAsync(request.RefreshToken, ct);
        return Result.Ok();
    }
}
