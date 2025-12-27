using OsLog.Application.Abstractions.Identity;
using OsLog.Application.Common.Result;

namespace OsLog.Application.UseCases.Autenticacao.ChangePassword;

public sealed class ChangePasswordUseCase : IChangePasswordUseCase
{
    private readonly IIdentityGateway _identity;
    public ChangePasswordUseCase(IIdentityGateway identity) => _identity = identity;

    public Task<Result> ExecuteAsync(ChangePasswordRequest request, CancellationToken ct = default)
        => _identity.ChangePasswordAsync(request.Email, request.SenhaAtual, request.NovaSenha, ct);
}
