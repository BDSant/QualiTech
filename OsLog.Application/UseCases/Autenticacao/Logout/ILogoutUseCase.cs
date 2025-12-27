using OsLog.Application.Common.Result;

namespace OsLog.Application.UseCases.Autenticacao.Logout;

public interface ILogoutUseCase
{
    Task<Result> ExecuteAsync(LogoutRequest request, CancellationToken ct = default);
}
