namespace OsLog.Application.DTOs.Users;

public sealed record UserDetailsResponse
{
    public string UserId { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool EmailConfirmed { get; init; }

    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<ClaimResultDto> Claims { get; init; } = Array.Empty<ClaimResultDto>();
}
