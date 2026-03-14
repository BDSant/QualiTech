using OsLog.Application.Common.Result;
using OsLog.Application.DTOs.Identity;
using OsLog.Application.DTOs.Users;
using OsLog.Application.Ports.Identity.Admin;

namespace OsLog.Application.UseCases.Users;

/// <summary>
/// Caso de uso: criação de usuário com atribuição opcional de roles e claims.
/// </summary>
public sealed class CreateUserUseCase
{
    private readonly IIdentityAdminGateway _identityAdminGateway;

    public CreateUserUseCase(IIdentityAdminGateway identityAdminGateway)
    {
        _identityAdminGateway = identityAdminGateway
            ?? throw new ArgumentNullException(nameof(identityAdminGateway));
    }

    public async Task<CreateUserUseCaseResult> ExecuteAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return CreateUserUseCaseResult.Validation("Request inválido.");

        var errors = ValidateRequest(request);
        if (errors.Count > 0)
            return CreateUserUseCaseResult.Validation(errors);

        var existingUsers = await _identityAdminGateway.GetAllUsersAsync(cancellationToken);

        var emailJaExiste = existingUsers.Any(u =>
            string.Equals(u.Email, request.Email.Trim(), StringComparison.OrdinalIgnoreCase));

        if (emailJaExiste)
        {
            return CreateUserUseCaseResult.Conflict(
                "Já existe um usuário cadastrado com este e-mail.");
        }

        var createResult = await _identityAdminGateway.CreateUserAsync(
            email: request.Email.Trim(),
            password: request.Password,
            ct: cancellationToken);

        if (!createResult.IsSuccess || string.IsNullOrWhiteSpace(createResult.Value))
        {
            return CreateUserUseCaseResult.IdentityError(ExtractErrors(createResult));
        }

        var userId = createResult.Value;

        var roles = (request.Roles ?? Array.Empty<string>())
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (roles.Length > 0)
        {
            foreach (var role in roles)
            {
                var ensureRoleResult = await _identityAdminGateway.EnsureRoleExistsAsync(
                    role,
                    cancellationToken);

                if (!ensureRoleResult.IsSuccess)
                    return CreateUserUseCaseResult.IdentityError(ExtractErrors(ensureRoleResult));
            }

            var replaceRolesResult = await _identityAdminGateway.ReplaceUserRolesAsync(
                userId,
                roles,
                cancellationToken);

            if (!replaceRolesResult.IsSuccess)
                return CreateUserUseCaseResult.IdentityError(ExtractErrors(replaceRolesResult));
        }

        var claims = (request.Claims ?? Array.Empty<ClaimDto>())
            .Where(c => c is not null &&
                        !string.IsNullOrWhiteSpace(c.Type) &&
                        !string.IsNullOrWhiteSpace(c.Value))
            .Select(c => new UserClaimDto
            {
                Type = c.Type.Trim(),
                Value = c.Value.Trim()
            })
            .Distinct(new UserClaimDtoComparer())
            .ToArray();

        if (claims.Length > 0)
        {
            var replaceClaimsResult = await _identityAdminGateway.ReplaceUserClaimsAsync(
                userId,
                claims,
                cancellationToken);

            if (!replaceClaimsResult.IsSuccess)
                return CreateUserUseCaseResult.IdentityError(ExtractErrors(replaceClaimsResult));
        }

        var response = new CreateUserResponse
        {
            UserId = userId,
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim(),
            Roles = roles,
            Claims = claims
                .Select(c => new ClaimResultDto
                {
                    Type = c.Type,
                    Value = c.Value
                })
                .ToArray()
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

        if (request.ConfirmPassword is not null &&
            !string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            errors.Add("Password e ConfirmPassword não conferem.");
        }

        if (request.Roles is not null && request.Roles.Any(r => string.IsNullOrWhiteSpace(r)))
            errors.Add("Lista de Roles contém valores inválidos.");

        if (request.Claims is not null &&
            request.Claims.Any(c => c is null ||
                                    string.IsNullOrWhiteSpace(c.Type) ||
                                    string.IsNullOrWhiteSpace(c.Value)))
        {
            errors.Add("Lista de Claims contém valores inválidos.");
        }

        return errors;
    }

    private static IReadOnlyCollection<string> ExtractErrors(Result result)
    {
        if (result.Errors is null || result.Errors.Count == 0)
            return new[] { "Erro de identidade não especificado." };

        return result.Errors
            .Select(e => e.Message)
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .ToArray();
    }

    private static IReadOnlyCollection<string> ExtractErrors(Result<string> result)
    {
        if (result.Errors is null || result.Errors.Count == 0)
            return new[] { "Erro de identidade não especificado." };

        return result.Errors
            .Select(e => e.Message)
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .ToArray();
    }

    private sealed class UserClaimDtoComparer : IEqualityComparer<UserClaimDto>
    {
        public bool Equals(UserClaimDto? x, UserClaimDto? y)
        {
            if (x is null && y is null) return true;
            if (x is null || y is null) return false;

            return string.Equals(x.Type, y.Type, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(x.Value, y.Value, StringComparison.Ordinal);
        }

        public int GetHashCode(UserClaimDto obj)
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
/// Resultado específico do caso de uso de criação de usuário.
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
        if (data is null)
            throw new ArgumentNullException(nameof(data));

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

    public static CreateUserUseCaseResult Fail(
        CreateUserErrorCode code,
        params string[] errors) =>
        new(
            succeeded: false,
            data: null,
            errors: (errors is { Length: > 0 }) ? errors : new[] { "Erro desconhecido." },
            errorCode: code == CreateUserErrorCode.None ? CreateUserErrorCode.IdentityError : code);

    public static CreateUserUseCaseResult Fail(
        CreateUserErrorCode code,
        IEnumerable<string> errors) =>
        new(
            succeeded: false,
            data: null,
            errors: (errors ?? Array.Empty<string>())
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .ToArray(),
            errorCode: code == CreateUserErrorCode.None ? CreateUserErrorCode.IdentityError : code);
}

/// <summary>
/// Códigos determinísticos de erro para o caso de uso de criação de usuário.
/// </summary>
public enum CreateUserErrorCode
{
    None = 0,
    Validation = 1,
    Conflict = 2,
    IdentityError = 3
}