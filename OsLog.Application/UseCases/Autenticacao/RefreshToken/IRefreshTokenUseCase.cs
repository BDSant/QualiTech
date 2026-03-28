using OsLog.Application.Common.Result;
using OsLog.Application.UseCases.Autenticacao.Common;

namespace OsLog.Application.UseCases.Autenticacao.RefreshToken;

public interface IRefreshTokenUseCase
{
    Task<Result<TokenResponse>> ExecuteAsync(RefreshTokenRequest request, CancellationToken ct = default);
}
