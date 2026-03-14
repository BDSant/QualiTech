using OsLog.Application.Common.Result;
using System.Security.Claims;

namespace OsLog.Application.Ports.Identity.Runtime;

public interface IIdentityGateway
{
    Task<IdentityUserDto?> FindByEmailAsync(
        string email,
        CancellationToken ct = default);

    Task<IdentityUserDto?> FindByIdAsync(
        string userId,
        CancellationToken ct = default);

    Task<bool> CheckPasswordAsync(
        string userId,
        string password,
        CancellationToken ct = default);

    Task<IReadOnlyList<string>> GetRolesAsync(
        string userId,
        CancellationToken ct = default);

    Task<IReadOnlyList<Claim>> GetClaimsAsync(
        string userId,
        CancellationToken ct = default);

    Task<Result> ChangePasswordAsync(
        string email,
        string currentPassword,
        string newPassword,
        CancellationToken ct = default);

    Task<Result> ResetPasswordAsync(
        string email,
        string newPassword,
        CancellationToken ct = default);
}