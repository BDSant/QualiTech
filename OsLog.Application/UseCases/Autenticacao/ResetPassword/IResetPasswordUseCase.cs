using OsLog.Application.Common.Result;

namespace OsLog.Application.UseCases.Autenticacao.ResetPassword;

public interface IResetPasswordUseCase
{
    Task<Result> ExecuteAsync(ResetPasswordRequest request, CancellationToken ct = default);
}
