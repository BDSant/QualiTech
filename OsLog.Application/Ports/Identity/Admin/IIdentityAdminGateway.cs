using OsLog.Application.Common.Result;
using OsLog.Application.DTOs.Identity;

namespace OsLog.Application.Ports.Identity.Admin;

public interface IIdentityAdminGateway
{
    Task<IReadOnlyCollection<UsuarioListDto>> GetAllUsersAsync(CancellationToken ct = default);

    Task<bool> UserExistsAsync(string userId, CancellationToken ct = default);

    Task<Result<string>> CreateUserAsync(
        string email,
        string password,
        CancellationToken ct = default);

    Task<Result> EnsureRoleExistsAsync(
        string roleName,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<string>> GetRolesAsync(
        CancellationToken ct = default);

    Task<Result> AddUserToRoleAsync(
        string userId,
        string roleName,
        CancellationToken ct = default);

    Task<Result> ReplaceUserRolesAsync(
        string userId,
        IEnumerable<string> roles,
        CancellationToken ct = default);

    Task<Result> RemoveUserFromRoleAsync(
        string userId,
        string roleName,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<string>> GetUserRolesAsync(
        string userId,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<UserClaimDto>> GetUserClaimsAsync(
        string userId,
        CancellationToken ct = default);

    Task<Result> AddClaimToUserAsync(
        string userId,
        string claimType,
        string claimValue,
        CancellationToken ct = default);

    Task<Result> ReplaceUserClaimsAsync(
        string userId,
        IEnumerable<UserClaimDto> claims,
        CancellationToken ct = default);

    Task<Result> RemoveClaimFromUserAsync(
        string userId,
        string claimType,
        string claimValue,
        CancellationToken ct = default);

    Task<UsuarioListDto?> GetUserByIdAsync(string userId, CancellationToken ct = default);
}