using OsLog.Application.Common.Result;
using OsLog.Application.DTOs.Auth;

namespace OsLog.Application.UseCases.Autenticacao.RefreshToken;

public interface IRefreshTokenUseCase
{
    Task<Result<TokenResponseDto>> ExecuteAsync(RefreshTokenRequest request, CancellationToken ct = default);
}
