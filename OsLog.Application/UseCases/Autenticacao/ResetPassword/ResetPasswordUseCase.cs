using OsLog.Application.Abstractions.Identity;
using OsLog.Application.Common.Result;

namespace OsLog.Application.UseCases.Autenticacao.ResetPassword;

public sealed class ResetPasswordUseCase : IResetPasswordUseCase
{
    private readonly IIdentityGateway _identity;
    public ResetPasswordUseCase(IIdentityGateway identity) => _identity = identity;

    public Task<Result> ExecuteAsync(ResetPasswordRequest request, CancellationToken ct = default)
        => _identity.ResetPasswordAsync(request.Email, request.NovaSenha, ct);
}
