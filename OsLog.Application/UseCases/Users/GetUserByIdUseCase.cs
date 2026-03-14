using OsLog.Application.DTOs.Users;
using OsLog.Application.Ports.Identity.Admin;

namespace OsLog.Application.UseCases.Users;

public sealed class GetUserByIdUseCase
{
    private readonly IIdentityAdminGateway _identityAdminGateway;

    public GetUserByIdUseCase(IIdentityAdminGateway identityAdminGateway)
    {
        _identityAdminGateway = identityAdminGateway ?? throw new ArgumentNullException(nameof(identityAdminGateway));
    }

    public async Task<GetUserByIdUseCaseResult> ExecuteAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return GetUserByIdUseCaseResult.Fail("UserId é obrigatório.");

        var user = await _identityAdminGateway.GetUserByIdAsync(userId.Trim(), cancellationToken);
        if (user is null)
            return GetUserByIdUseCaseResult.NotFound();

        var rolesResult = await _identityAdminGateway.GetUserRolesAsync(user.Id, cancellationToken);

        var claimsResult = await _identityAdminGateway.GetUserClaimsAsync(user.Id, cancellationToken);

        var response = new UserDetailsResponse
        {
            UserId = user.Id,
            UserName = user.Nome,
            Email = user.Email,
            EmailConfirmed = user.EmailConfirmado,
            Roles = rolesResult,
            Claims = claimsResult
                .Select(c => new ClaimResultDto 
                { 
                    Type = c.Type, 
                    Value = c.Value 
                })
                .ToArray()
        };

        return GetUserByIdUseCaseResult.Ok(response);
    }
}

public sealed class GetUserByIdUseCaseResult
{
    private GetUserByIdUseCaseResult(
        bool succeeded,
        bool isNotFound,
        UserDetailsResponse? data,
        IReadOnlyCollection<string> errors)
    {
        Succeeded = succeeded;
        IsNotFound = isNotFound;
        Data = data;
        Errors = errors;
    }

    public bool Succeeded { get; }
    public bool IsNotFound { get; }
    public UserDetailsResponse? Data { get; }
    public IReadOnlyCollection<string> Errors { get; }

    public static GetUserByIdUseCaseResult Ok(UserDetailsResponse data) =>
        new(true, false, data, Array.Empty<string>());

    public static GetUserByIdUseCaseResult NotFound() =>
        new(false, true, null, Array.Empty<string>());

    public static GetUserByIdUseCaseResult Fail(params string[] errors) =>
        new(false, false, null, (errors?.Length > 0 ? errors : new[] { "Erro desconhecido." }));

    public static GetUserByIdUseCaseResult Fail(IEnumerable<string> errors) =>
        new(false, false, null, (errors ?? Array.Empty<string>()).ToArray());
}
