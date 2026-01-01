// File: OsLog.Application/Interfaces/IIdentityAdminGateway.cs
using System.Security.Claims;

namespace OsLog.Application.Interfaces;

/// <summary>
/// Gateway administrativo de Identity.
/// 
/// Objetivo:
/// - Expor operações administrativas (criação de usuário, roles e claims) para a camada Application
///   sem acoplar a Application ao ASP.NET Identity (UserManager/RoleManager) ou a Infraestrutura.
/// 
/// Regras:
/// - A Application consome esta interface.
/// - A Infrastructure implementa (ex.: usando UserManager/RoleManager).
/// - IDs e contratos são simples (string) para manter desacoplamento.
/// </summary>
public interface IIdentityAdminGateway
{
    /// <summary>
    /// Obtém um usuário pelo e-mail (normalizado ou não, a implementação decide).
    /// Retorna null quando não encontrado.
    /// </summary>
    Task<IdentityUserData?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um usuário pelo Id.
    /// Retorna null quando não encontrado.
    /// </summary>
    Task<IdentityUserData?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria um usuário com senha.
    /// </summary>
    Task<IdentityOperationResult<IdentityUserData>> CreateUserAsync(
        string userName,
        string email,
        string password,
        bool emailConfirmed = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Garante que uma role exista. Se já existir, deve ser idempotente (sucesso).
    /// </summary>
    Task<IdentityOperationResult> EnsureRoleExistsAsync(
        string roleName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona roles ao usuário (a implementação pode garantir a existência das roles ou falhar caso não existam).
    /// Deve ser idempotente: adicionar role já existente no usuário não deve gerar erro.
    /// </summary>
    Task<IdentityOperationResult> AddUserToRolesAsync(
        string userId,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Substitui todas as roles do usuário pelas roles informadas (opcional, mas útil em cenários administrativos).
    /// </summary>
    Task<IdentityOperationResult> ReplaceUserRolesAsync(
        string userId,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona claims ao usuário. Deve ser idempotente.
    /// </summary>
    Task<IdentityOperationResult> AddClaimsAsync(
        string userId,
        IEnumerable<Claim> claims,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Substitui claims de um determinado tipo (ou conjunto) por novos valores.
    /// Ex.: substituir "unidade" antigas pelas atuais.
    /// </summary>
    Task<IdentityOperationResult> ReplaceClaimsAsync(
        string userId,
        IEnumerable<Claim> claims,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove claims do usuário (quando existirem). Deve ser idempotente.
    /// </summary>
    Task<IdentityOperationResult> RemoveClaimsAsync(
        string userId,
        IEnumerable<Claim> claims,
        CancellationToken cancellationToken = default);

    Task<IdentityOperationResult<IReadOnlyCollection<string>>> GetUserRolesAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<IdentityOperationResult<IReadOnlyCollection<Claim>>> GetUserClaimsAsync(
        string userId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Representação mínima de um usuário para consumo na Application, sem depender de Identity.
/// </summary>
public sealed record IdentityUserData(
    string Id,
    string UserName,
    string Email,
    bool EmailConfirmed);

/// <summary>
/// Resultado de operação administrativa (sucesso/erros) sem acoplamento a IdentityResult.
/// </summary>
public class IdentityOperationResult
{
    public bool Succeeded { get; init; }
    public IdentityOperationErrorCode ErrorCode { get; init; } = IdentityOperationErrorCode.None;
    public IReadOnlyCollection<string> Errors { get; init; } = Array.Empty<string>();

    public static IdentityOperationResult Success() =>
        new() { Succeeded = true, ErrorCode = IdentityOperationErrorCode.None };

    public static IdentityOperationResult Failure(params string[] errors) =>
        Failure(IdentityOperationErrorCode.IdentityError, errors);

    public static IdentityOperationResult Failure(IEnumerable<string> errors) =>
        Failure(IdentityOperationErrorCode.IdentityError, errors);

    public static IdentityOperationResult Failure(IdentityOperationErrorCode code, params string[] errors) =>
        new()
        {
            Succeeded = false,
            ErrorCode = code,
            Errors = errors?.Length > 0 ? errors : new[] { "Unknown error." }
        };

    public static IdentityOperationResult Failure(IdentityOperationErrorCode code, IEnumerable<string> errors) =>
        new()
        {
            Succeeded = false,
            ErrorCode = code,
            Errors = (errors ?? Array.Empty<string>()).ToArray()
        };
}
/// <summary>
/// Resultado com payload (ex.: criação de usuário), preservando erros padronizados.
/// </summary>
public sealed class IdentityOperationResult<T> : IdentityOperationResult
{
    public T? Data { get; init; }

    public static IdentityOperationResult<T> Success(T data) =>
        new() { Succeeded = true, ErrorCode = IdentityOperationErrorCode.None, Data = data };

    public static new IdentityOperationResult<T> Failure(params string[] errors) =>
        Failure(IdentityOperationErrorCode.IdentityError, errors);

    public static new IdentityOperationResult<T> Failure(IEnumerable<string> errors) =>
        Failure(IdentityOperationErrorCode.IdentityError, errors);

    public static new IdentityOperationResult<T> Failure(IdentityOperationErrorCode code, params string[] errors) =>
        new()
        {
            Succeeded = false,
            ErrorCode = code,
            Errors = errors?.Length > 0 ? errors : new[] { "Unknown error." }
        };

    public static new IdentityOperationResult<T> Failure(IdentityOperationErrorCode code, IEnumerable<string> errors) =>
        new()
        {
            Succeeded = false,
            ErrorCode = code,
            Errors = (errors ?? Array.Empty<string>()).ToArray()
        };
}
public enum IdentityOperationErrorCode
{
    None = 0,
    Conflict = 1,
    NotFound = 2,
    Validation = 3,
    IdentityError = 4
}
