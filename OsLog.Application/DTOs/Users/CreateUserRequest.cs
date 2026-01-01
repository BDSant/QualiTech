// File: OsLog.Application/DTOs/Users/CreateUserRequest.cs
using System.Security.Claims;

namespace OsLog.Application.DTOs.Users;

/// <summary>
/// Contrato de entrada do caso de uso de criação de usuário.
/// A camada API deve enviar este contrato ao UseCase.
/// </summary>
public sealed record CreateUserRequest
{
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Confirmação opcional (útil para validações de request na Application/API).
    /// </summary>
    public string? ConfirmPassword { get; init; }

    /// <summary>
    /// Se true, o usuário já nasce com email confirmado.
    /// </summary>
    public bool EmailConfirmed { get; init; } = false;

    /// <summary>
    /// Roles iniciais do usuário (ex.: Master, Admin, Tecnico).
    /// </summary>
    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Claims iniciais do usuário.
    /// Ex.: ("empresa_id","1"), ("unidade_id","10"), etc.
    /// </summary>
    public IReadOnlyCollection<ClaimDto> Claims { get; init; } = Array.Empty<ClaimDto>();
}

/// <summary>
/// DTO neutro para claim (evita acoplamento com Claim em camadas externas se quiser serializar facilmente).
/// </summary>
public sealed record ClaimDto
{
    public string Type { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;

    public Claim ToClaim() => new(Type, Value);
}

