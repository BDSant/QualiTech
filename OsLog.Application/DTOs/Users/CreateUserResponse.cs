// File: OsLog.Application/DTOs/Users/CreateUserResponse.cs
namespace OsLog.Application.DTOs.Users;

/// <summary>
/// Contrato de saída do caso de uso de criação de usuário.
/// </summary>
public sealed record CreateUserResponse
{
    public string UserId { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;

    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<ClaimResultDto> Claims { get; init; } = Array.Empty<ClaimResultDto>();
}

public sealed record ClaimResultDto
{
    public string Type { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

