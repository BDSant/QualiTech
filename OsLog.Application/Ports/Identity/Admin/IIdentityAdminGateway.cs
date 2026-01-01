using System.Security.Claims;

namespace OsLog.Application.Ports.Identity.Admin;

public interface IIdentityAdminGateway
{
    Task<IdentityUserData?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IdentityUserData?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);

    Task<IdentityOperationResult<IdentityUserData>> CreateUserAsync(
        string userName,
        string email,
        string password,
        bool emailConfirmed = false,
        CancellationToken cancellationToken = default);

    Task<IdentityOperationResult> EnsureRoleExistsAsync(
        string roleName,
        CancellationToken cancellationToken = default);

    Task<IdentityOperationResult> AddUserToRolesAsync(
        string userId,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default);

    Task<IdentityOperationResult> ReplaceUserRolesAsync(
        string userId,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default);

    Task<IdentityOperationResult> AddClaimsAsync(
        string userId,
        IEnumerable<Claim> claims,
        CancellationToken cancellationToken = default);

    Task<IdentityOperationResult> ReplaceClaimsAsync(
        string userId,
        IEnumerable<Claim> claims,
        CancellationToken cancellationToken = default);

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
