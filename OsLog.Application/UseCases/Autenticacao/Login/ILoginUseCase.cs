using OsLog.Application.Common.Result;
using OsLog.Application.DTOs.Auth;

namespace OsLog.Application.UseCases.Autenticacao.Login;

public interface ILoginUseCase
{
    Task<Result<TokenResponseDto>> ExecuteAsync(LoginRequest request, CancellationToken ct = default);
}
