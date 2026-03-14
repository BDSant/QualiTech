using Microsoft.AspNetCore.Identity;
using OsLog.Application.Common.Result;
using OsLog.Application.Common.Security.ErrorCodes;
using OsLog.Application.Ports.Identity.Runtime;
using System.Security.Claims;

namespace OsLog.Infrastructure.Identity.Gateway;

public sealed class IdentityGateway : IIdentityGateway
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IdentityGateway(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IdentityUserDto?> FindByEmailAsync(
        string email,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var user = await _userManager.FindByEmailAsync(email);

        return user is null
            ? null
            : new IdentityUserDto(
                user.Id,
                user.Email ?? email);
    }

    public async Task<IdentityUserDto?> FindByIdAsync(
        string userId,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(userId);

        return user is null
            ? null
            : new IdentityUserDto(
                user.Id,
                user.Email ?? string.Empty);
    }

    public async Task<bool> CheckPasswordAsync(
        string userId,
        string password,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return false;

        var result = await _signInManager.CheckPasswordSignInAsync(
            user,
            password,
            lockoutOnFailure: true);

        return result.Succeeded;
    }

    public async Task<IReadOnlyList<string>> GetRolesAsync(
        string userId,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Array.Empty<string>();

        var roles = await _userManager.GetRolesAsync(user);
        return roles.ToList();
    }

    public async Task<IReadOnlyList<Claim>> GetClaimsAsync(
        string userId,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Array.Empty<Claim>();

        var claims = await _userManager.GetClaimsAsync(user);
        return claims.ToList();
    }

    public async Task<Result> ChangePasswordAsync(
        string email,
        string currentPassword,
        string newPassword,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Result.Fail(
                new AppError(
                    AuthErrorCodes.UserNotFound,
                    "Usuário não encontrado.",
                    ErrorType.NotFound));
        }

        var result = await _userManager.ChangePasswordAsync(
            user,
            currentPassword,
            newPassword);

        if (result.Succeeded)
            return Result.Ok();

        var errors = result.Errors
            .Select(e => new AppError(
                AuthErrorCodes.ChangePasswordFailed,
                e.Description,
                ErrorType.Validation))
            .ToArray();

        return Result.Fail(errors);
    }

    public async Task<Result> ResetPasswordAsync(
        string email,
        string newPassword,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Result.Fail(
                new AppError(
                    AuthErrorCodes.UserNotFound,
                    "Usuário não encontrado.",
                    ErrorType.NotFound));
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var result = await _userManager.ResetPasswordAsync(
            user,
            token,
            newPassword);

        if (result.Succeeded)
            return Result.Ok();

        var errors = result.Errors
            .Select(e => new AppError(
                AuthErrorCodes.ResetPasswordFailed,
                e.Description,
                ErrorType.Validation))
            .ToArray();

        return Result.Fail(errors);
    }
}