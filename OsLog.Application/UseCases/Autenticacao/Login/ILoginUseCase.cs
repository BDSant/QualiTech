using OsLog.Application.Common.Result;
using OsLog.Application.UseCases.Autenticacao.Common;

namespace OsLog.Application.UseCases.Autenticacao.Login;

public interface ILoginUseCase
{
    Task<Result<TokenResponse>> ExecuteAsync(LoginRequest request, CancellationToken ct = default);
}
