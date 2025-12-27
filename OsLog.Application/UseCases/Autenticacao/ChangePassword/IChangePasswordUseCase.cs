using OsLog.Application.Common.Result;

namespace OsLog.Application.UseCases.Autenticacao.ChangePassword;

public interface IChangePasswordUseCase
{
    Task<Result> ExecuteAsync(ChangePasswordRequest request, CancellationToken ct = default);
}
