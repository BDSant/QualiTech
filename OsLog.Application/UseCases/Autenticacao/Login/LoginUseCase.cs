using OsLog.Application.Common.Result;
using OsLog.Application.Common.Security.ErrorCodes;
using OsLog.Application.Ports.Identity.Runtime;
using OsLog.Application.Ports.Security;
using OsLog.Application.UseCases.Autenticacao.Common;

namespace OsLog.Application.UseCases.Autenticacao.Login;

public sealed class LoginUseCase : ILoginUseCase
{
    private readonly IIdentityGateway _identity;
    private readonly IJwtTokenService _jwt;
    private readonly IUsuarioAutenticadoResolver _usuarioAutenticadoResolver;

    public LoginUseCase(
        IIdentityGateway identity,
        IJwtTokenService jwt,
        IUsuarioAutenticadoResolver usuarioAutenticadoResolver)
    {
        _identity = identity;
        _jwt = jwt;
        _usuarioAutenticadoResolver = usuarioAutenticadoResolver;
    }

    public async Task<Result<TokenResponse>> ExecuteAsync(
        LoginRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return (Result<TokenResponse>)Result.Fail(new AppError(
                AuthErrorCodes.EmailRequired,
                "E-mail é obrigatório.",
                ErrorType.Validation,
                "email"));
        }

        if (string.IsNullOrWhiteSpace(request.Senha))
        {
            return (Result<TokenResponse>)Result.Fail(new AppError(
                AuthErrorCodes.PasswordRequired,
                "Senha é obrigatória.",
                ErrorType.Validation,
                "senha"));
        }

        var user = await _identity.FindByEmailAsync(request.Email, ct);

        if (user is null)
        {
            return (Result<TokenResponse>)Result.Fail(new AppError(
                AuthErrorCodes.InvalidCredentials,
                "Usuário ou senha inválidos.",
                ErrorType.Unauthorized));
        }

        // TODO: Implementar AD
        // if (!user.IsActiveDirectory)

        var ok = await _identity.CheckPasswordAsync(user.Id, request.Senha, ct);

        if (!ok)
        {
            return (Result<TokenResponse>)Result.Fail(new AppError(
                AuthErrorCodes.InvalidCredentials,
                "Usuário ou senha inválidos.",
                ErrorType.Unauthorized));
        }

        // TODO: Verificar se o usuário está ativo, bloqueado, etc.
        // Exemplo:
        // if (!user.IsActive)
        //     return Result.Fail(new AppError(
        //         AuthErrorCodes.UserInactive,
        //         "Usuário está inativo.",
        //         ErrorType.Forbidden));

        // if (user.IsLockedOut)
        //     return Result.Fail(new AppError(
        //         AuthErrorCodes.UserInactive,
        //         "Usuário bloqueado temporariamente por tentativas inválidas.",
        //         ErrorType.Forbidden));

        var usuarioId = await _usuarioAutenticadoResolver.ObterUsuarioIdAsync(user.Id, ct);

        if (!usuarioId.HasValue)
        {
            return (Result<TokenResponse>)Result.Fail(new AppError(
                AuthErrorCodes.InvalidCredentials,
                "Usuário autenticado sem vínculo com o cadastro interno de acesso.",
                ErrorType.Unauthorized));
        }

        var roles = await _identity.GetRolesAsync(user.Id, ct);
        var claims = await _identity.GetClaimsAsync(user.Id, ct);

        var tokens = await _jwt.GenerateTokensAsync(
            user.Id,
            user.Email,
            roles,
            usuarioId.Value,
            claims,
            ct);

        return Result<TokenResponse>.Ok(tokens);
    }
}