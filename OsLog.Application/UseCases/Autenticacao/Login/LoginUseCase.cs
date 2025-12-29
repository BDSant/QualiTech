using OsLog.Application.Abstractions.Identity;
using OsLog.Application.Abstractions.Security;
using OsLog.Application.Common.Result;
using OsLog.Application.Common.Security.ErrorCodes;
using OsLog.Application.DTOs.Auth;
using System;

namespace OsLog.Application.UseCases.Autenticacao.Login;

public sealed class LoginUseCase : ILoginUseCase
{
    private readonly IIdentityGateway _identity;
    private readonly IJwtTokenService _jwt;

    public LoginUseCase(IIdentityGateway identity, IJwtTokenService jwt)
    {
        _identity = identity;
        _jwt = jwt;
    }

    public async Task<Result<TokenResponseDto>> ExecuteAsync(LoginRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return Result<TokenResponseDto>.Fail(new AppError(AuthErrorCodes.EmailRequired, "E-mail é obrigatório.", ErrorType.Validation, "email"));

        if (string.IsNullOrWhiteSpace(request.Senha))
            return Result<TokenResponseDto>.Fail(new AppError(AuthErrorCodes.PasswordRequired, "Senha é obrigatória.", ErrorType.Validation, "senha"));

        var user = await _identity.FindByEmailAsync(request.Email, ct);
        if (user is null)
            return Result<TokenResponseDto>.Fail(new AppError(AuthErrorCodes.InvalidCredentials, "Usuário ou senha inválidos.", ErrorType.Unauthorized));

        // TODO: Implementar AD
        // if (!user.IsActiveDirectory)


        var ok = await _identity.CheckPasswordAsync(user.Id, request.Senha, ct);
        if (!ok)
            return Result<TokenResponseDto>.Fail(new AppError(AuthErrorCodes.InvalidCredentials, "Usuário ou senha inválidos.", ErrorType.Unauthorized));

        // TODO: Verificar se o usuário está ativo, bloqueado, etc. e Active Directory authentication
        // Exemplo:
        // if (!user.IsActive)
        //     return Result<TokenResponseDto>.Fail(new AppError(AuthErrorCodes.UserInactive,
        //         "Usuário está inativo.", ErrorType.Forbidden));

        // if (!user.IsLockedOut)
        //     return Result<TokenResponseDto>.Fail(new AppError(AuthErrorCodes.UserInactive,
        //         "Usuário bloqueado temporariamente por tentativas inválidas.", ErrorType.Forbidden));

        


        var roles = await _identity.GetRolesAsync(user.Id, ct);
        var claims = await _identity.GetClaimsAsync(user.Id, ct);

        var tokens = await _jwt.GenerateTokensAsync(user.Id, user.Email, roles, claims, ct);
        return Result<TokenResponseDto>.Ok(tokens);
    }
}
