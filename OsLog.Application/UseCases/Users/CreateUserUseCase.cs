using System.Security.Claims;
using OsLog.Application.DTOs.Users;
using OsLog.Application.Ports.Identity.Admin;

namespace OsLog.Application.UseCases.Users;

/// <summary>
/// Caso de uso: Criação de usuário com atribuição opcional de roles e claims.
///
/// Clean Architecture:
/// - Depende apenas de abstrações (IIdentityAdminGateway)
/// - Não referencia ASP.NET Identity (UserManager/RoleManager) nem EF Core
/// </summary>
public sealed class CreateUserUseCase
{
    private readonly IIdentityAdminGateway _identityAdminGateway;

    public CreateUserUseCase(IIdentityAdminGateway identityAdminGateway)
    {
        _identityAdminGateway = identityAdminGateway ?? throw new ArgumentNullException(nameof(identityAdminGateway));
    }

    /// <summary>
    /// Executa o caso de uso.
    /// </summary>
    public async Task<CreateUserUseCaseResult> ExecuteAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return CreateUserUseCaseResult.Fail(CreateUserErrorCode.Validation, "Request inválido.");

        // Validação mínima do request
        var errors = ValidateRequest(request);
        if (errors.Count > 0)
            return CreateUserUseCaseResult.Fail(CreateUserErrorCode.Validation, errors);

        // Garantir unicidade por e-mail
        var existingUser = await _identityAdminGateway.GetUserByEmailAsync(request.Email, cancellationToken);
        if (existingUser is not null)
            return CreateUserUseCaseResult.Fail(CreateUserErrorCode.Conflict, "Já existe um usuário cadastrado com este e-mail.");

        // Criar usuário
        var createResult = await _identityAdminGateway.CreateUserAsync(
            userName: request.UserName.Trim(),
            email: request.Email.Trim(),
            password: request.Password,
            emailConfirmed: request.EmailConfirmed,
            cancellationToken: cancellationToken);

        if (!createResult.Succeeded || createResult.Data is null)
        {
            var mapped = createResult.ErrorCode == IdentityOperationErrorCode.Conflict
                ? CreateUserErrorCode.Conflict
                : CreateUserErrorCode.IdentityError;

            return CreateUserUseCaseResult.Fail(mapped, createResult.Errors);
        }

        var created = createResult.Data;

        // Roles (opcional)
        var roles = (request.Roles ?? Array.Empty<string>())
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (roles.Length > 0)
        {
            foreach (var role in roles)
            {
                var ensureRole = await _identityAdminGateway.EnsureRoleExistsAsync(role, cancellationToken);
                if (!ensureRole.Succeeded)
                    return CreateUserUseCaseResult.Fail(CreateUserErrorCode.IdentityError, ensureRole.Errors);
            }

            var addRoles = await _identityAdminGateway.AddUserToRolesAsync(created.Id, roles, cancellationToken);
            if (!addRoles.Succeeded)
                return CreateUserUseCaseResult.Fail(CreateUserErrorCode.IdentityError, addRoles.Errors);
        }

        // Claims (opcional)
        var claims = (request.Claims ?? Array.Empty<ClaimDto>())
            .Where(c => c is not null && !string.IsNullOrWhiteSpace(c.Type) && c.Value is not null)
            .Select(c => new Claim(c!.Type.Trim(), c.Value.Trim()))
            .Distinct(new ClaimTypeValueComparer())
            .ToArray();

        if (claims.Length > 0)
        {
            var addClaims = await _identityAdminGateway.AddClaimsAsync(created.Id, claims, cancellationToken);
            if (!addClaims.Succeeded)
                return CreateUserUseCaseResult.Fail(CreateUserErrorCode.IdentityError, addClaims.Errors);
        }

        // Resposta
        var response = new CreateUserResponse
        {
            UserId = created.Id,
            UserName = created.UserName,
            Email = created.Email,
            Roles = roles,
            Claims = claims.Select(c => new ClaimResultDto { Type = c.Type, Value = c.Value }).ToArray()
        };

        return CreateUserUseCaseResult.Ok(response);
    }

    private static List<string> ValidateRequest(CreateUserRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.UserName))
            errors.Add("UserName é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add("Email é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Password))
            errors.Add("Password é obrigatório.");

        // Validação simples de confirmação (opcional)
        if (request.ConfirmPassword is not null && request.Password != request.ConfirmPassword)
            errors.Add("Password e ConfirmPassword não conferem.");

        // Evitar roles vazias
        if (request.Roles is not null && request.Roles.Any(r => string.IsNullOrWhiteSpace(r)))
            errors.Add("Lista de Roles contém valores inválidos.");

        // Evitar claims inválidas
        if (request.Claims is not null && request.Claims.Any(c => c is null || string.IsNullOrWhiteSpace(c.Type)))
            errors.Add("Lista de Claims contém valores inválidos.");

        return errors;
    }

    private sealed class ClaimTypeValueComparer : IEqualityComparer<Claim>
    {
        public bool Equals(Claim? x, Claim? y)
        {
            if (x is null && y is null) return true;
            if (x is null || y is null) return false;

            return string.Equals(x.Type, y.Type, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(x.Value, y.Value, StringComparison.Ordinal);
        }

        public int GetHashCode(Claim obj)
        {
            unchecked
            {
                var h1 = StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Type ?? string.Empty);
                var h2 = StringComparer.Ordinal.GetHashCode(obj.Value ?? string.Empty);
                return (h1 * 397) ^ h2;
            }
        }
    }
}

/// <summary>
/// Resultado específico do caso de uso.
/// Mantém o contrato simples; a API pode mapear para HTTP (200/201/400/409).
/// 
/// Caso seu projeto já possua um Result<T> consolidado, você pode substituir este tipo
/// por Result<CreateUserResponse> e remover esta classe.
/// </summary>
public sealed class CreateUserUseCaseResult
{
    private CreateUserUseCaseResult(
        bool succeeded,
        CreateUserResponse? data,
        IReadOnlyCollection<string> errors,
        CreateUserErrorCode errorCode)
    {
        Succeeded = succeeded;
        Data = data;
        Errors = errors ?? Array.Empty<string>();
        ErrorCode = errorCode;
    }

    public bool Succeeded { get; }
    public CreateUserResponse? Data { get; }
    public IReadOnlyCollection<string> Errors { get; }
    public CreateUserErrorCode ErrorCode { get; }

    public static CreateUserUseCaseResult Ok(CreateUserResponse data)
    {
        if (data is null) throw new ArgumentNullException(nameof(data));
        return new CreateUserUseCaseResult(
            succeeded: true,
            data: data,
            errors: Array.Empty<string>(),
            errorCode: CreateUserErrorCode.None);
    }

    public static CreateUserUseCaseResult Validation(params string[] errors) =>
        Fail(CreateUserErrorCode.Validation, errors);

    public static CreateUserUseCaseResult Validation(IEnumerable<string> errors) =>
        Fail(CreateUserErrorCode.Validation, errors);

    public static CreateUserUseCaseResult Conflict(params string[] errors) =>
        Fail(CreateUserErrorCode.Conflict, errors);

    public static CreateUserUseCaseResult Conflict(IEnumerable<string> errors) =>
        Fail(CreateUserErrorCode.Conflict, errors);

    public static CreateUserUseCaseResult IdentityError(params string[] errors) =>
        Fail(CreateUserErrorCode.IdentityError, errors);

    public static CreateUserUseCaseResult IdentityError(IEnumerable<string> errors) =>
        Fail(CreateUserErrorCode.IdentityError, errors);

    public static CreateUserUseCaseResult Fail(CreateUserErrorCode code, params string[] errors) =>
        new(
            succeeded: false,
            data: null,
            errors: (errors is { Length: > 0 })
                ? errors
                : new[] { "Erro desconhecido." },
            errorCode: code == CreateUserErrorCode.None ? CreateUserErrorCode.IdentityError : code);

    public static CreateUserUseCaseResult Fail(CreateUserErrorCode code, IEnumerable<string> errors) =>
        new(
            succeeded: false,
            data: null,
            errors: (errors ?? Array.Empty<string>()).Where(e => !string.IsNullOrWhiteSpace(e)).ToArray(),
            errorCode: code == CreateUserErrorCode.None ? CreateUserErrorCode.IdentityError : code);
}


/// <summary>
/// Códigos determinísticos de erro para o caso de uso de criação de usuário.
/// A API usa isso para mapear corretamente HTTP (ex.: Conflict -> 409).
/// </summary>
public enum CreateUserErrorCode
{
    None = 0,
    Validation = 1,
    Conflict = 2,
    IdentityError = 3
}





