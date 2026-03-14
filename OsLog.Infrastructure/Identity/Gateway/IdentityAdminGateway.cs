using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OsLog.Application.Common.Result;
using OsLog.Application.Common.Security.ErrorCodes;
using OsLog.Application.DTOs.Identity;
using OsLog.Application.Ports.Identity.Admin;
using System.Security.Claims;

namespace OsLog.Infrastructure.Identity.Gateway;

public sealed class IdentityAdminGateway : IIdentityAdminGateway
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public IdentityAdminGateway(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IReadOnlyCollection<UsuarioListDto>> GetAllUsersAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var usuarios = await _userManager.Users
            .AsNoTracking()
            .OrderBy(u => u.UserName)
            .Select(u => new UsuarioListDto
            {
                Id = u.Id,
                Nome = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                EmailConfirmado = u.EmailConfirmed,
                Ativo = !u.LockoutEnabled || u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow
            })
            .ToListAsync(ct);

        return usuarios;
    }

    public async Task<bool> UserExistsAsync(string userId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(userId))
            return false;

        var user = await _userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        return user is not null;
    }

    public async Task<Result<string>> CreateUserAsync(
        string email,
        string password,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            return Result<string>.Fail(
                new AppError(
                    AuthErrorCodes.UserAlreadyExists,
                    "Já existe um usuário com este e-mail.",
                    ErrorType.Conflict));
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
            return Result<string>.Ok(user.Id);

        var errors = result.Errors
            .Select(e => new AppError(
                AuthErrorCodes.CreateUserFailed,
                e.Description,
                ErrorType.Validation))
            .ToArray();

        return Result<string>.Fail(errors);
    }

    public async Task<Result> EnsureRoleExistsAsync(string roleName, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(roleName))
        {
            return Result.Fail(
                new AppError(
                    AuthErrorCodes.InvalidRole,
                    "O nome da role é obrigatório.",
                    ErrorType.Validation));
        }

        var exists = await _roleManager.RoleExistsAsync(roleName);
        if (exists)
            return Result.Ok();

        var result = await _roleManager.CreateAsync(new IdentityRole(roleName));

        if (result.Succeeded)
            return Result.Ok();

        var errors = result.Errors
            .Select(e => new AppError(
                AuthErrorCodes.CreateRoleFailed,
                e.Description,
                ErrorType.Validation))
            .ToArray();

        return Result.Fail(errors);
    }

    public async Task<IReadOnlyCollection<string>> GetRolesAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var roles = await _roleManager.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => r.Name!)
            .ToListAsync(ct);

        return roles;
    }

    public async Task<Result> AddUserToRoleAsync(
        string userId,
        string roleName,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var user = await FindUserAsync(userId);
        if (user is null)
        {
            return Result.Fail(
                new AppError(
                    AuthErrorCodes.UserNotFound,
                    "Usuário não encontrado.",
                    ErrorType.NotFound));
        }

        var roleExists = await _roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            return Result.Fail(
                new AppError(
                    AuthErrorCodes.RoleNotFound,
                    "Role não encontrada.",
                    ErrorType.NotFound));
        }

        var alreadyInRole = await _userManager.IsInRoleAsync(user, roleName);
        if (alreadyInRole)
            return Result.Ok();

        var result = await _userManager.AddToRoleAsync(user, roleName);

        if (result.Succeeded)
            return Result.Ok();

        var errors = result.Errors
            .Select(e => new AppError(
                AuthErrorCodes.AddRoleFailed,
                e.Description,
                ErrorType.Validation))
            .ToArray();

        return Result.Fail(errors);
    }

    public async Task<Result> ReplaceUserRolesAsync(
        string userId,
        IEnumerable<string> roles,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var user = await FindUserAsync(userId);
        if (user is null)
        {
            return Result.Fail(
                new AppError(
                    AuthErrorCodes.UserNotFound,
                    "Usuário não encontrado.",
                    ErrorType.NotFound));
        }

        var novasRoles = roles
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var role in novasRoles)
        {
            var exists = await _roleManager.RoleExistsAsync(role);
            if (!exists)
            {
                return Result.Fail(
                    new AppError(
                        AuthErrorCodes.RoleNotFound,
                        $"Role '{role}' não encontrada.",
                        ErrorType.NotFound));
            }
        }

        var atuais = await _userManager.GetRolesAsync(user);

        var paraRemover = atuais
            .Except(novasRoles, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var paraAdicionar = novasRoles
            .Except(atuais, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (paraRemover.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, paraRemover);
            if (!removeResult.Succeeded)
            {
                var errors = removeResult.Errors
                    .Select(e => new AppError(
                        AuthErrorCodes.RemoveRoleFailed,
                        e.Description,
                        ErrorType.Validation))
                    .ToArray();

                return Result.Fail(errors);
            }
        }

        if (paraAdicionar.Count > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(user, paraAdicionar);
            if (!addResult.Succeeded)
            {
                var errors = addResult.Errors
                    .Select(e => new AppError(
                        AuthErrorCodes.AddRoleFailed,
                        e.Description,
                        ErrorType.Validation))
                    .ToArray();

                return Result.Fail(errors);
            }
        }

        return Result.Ok();
    }

    public async Task<Result> RemoveUserFromRoleAsync(
        string userId,
        string roleName,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var user = await FindUserAsync(userId);
        if (user is null)
        {
            return Result.Fail(
                new AppError(
                    AuthErrorCodes.UserNotFound,
                    "Usuário não encontrado.",
                    ErrorType.NotFound));
        }

        var inRole = await _userManager.IsInRoleAsync(user, roleName);
        if (!inRole)
            return Result.Ok();

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);

        if (result.Succeeded)
            return Result.Ok();

        var errors = result.Errors
            .Select(e => new AppError(
                AuthErrorCodes.RemoveRoleFailed,
                e.Description,
                ErrorType.Validation))
            .ToArray();

        return Result.Fail(errors);
    }

    public async Task<IReadOnlyCollection<string>> GetUserRolesAsync(
        string userId,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var user = await FindUserAsync(userId);
        if (user is null)
            return Array.Empty<string>();

        var roles = await _userManager.GetRolesAsync(user);
        return roles.ToList();
    }

    public async Task<IReadOnlyCollection<UserClaimDto>> GetUserClaimsAsync(
        string userId,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var user = await FindUserAsync(userId);
        if (user is null)
            return Array.Empty<UserClaimDto>();

        var claims = await _userManager.GetClaimsAsync(user);

        return claims
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Value)
            .Select(c => new UserClaimDto
            {
                Type = c.Type,
                Value = c.Value
            })
            .ToList();
    }

    public async Task<Result> AddClaimToUserAsync(
        string userId,
        string claimType,
        string claimValue,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var user = await FindUserAsync(userId);
        if (user is null)
        {
            return Result.Fail(
                new AppError(
                    AuthErrorCodes.UserNotFound,
                    "Usuário não encontrado.",
                    ErrorType.NotFound));
        }

        var claims = await _userManager.GetClaimsAsync(user);
        var exists = claims.Any(c =>
            c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase) &&
            c.Value.Equals(claimValue, StringComparison.Ordinal));

        if (exists)
            return Result.Ok();

        var result = await _userManager.AddClaimAsync(user, new Claim(claimType, claimValue));

        if (result.Succeeded)
            return Result.Ok();

        var errors = result.Errors
            .Select(e => new AppError(
                AuthErrorCodes.AddClaimFailed,
                e.Description,
                ErrorType.Validation))
            .ToArray();

        return Result.Fail(errors);
    }

    public async Task<Result> ReplaceUserClaimsAsync(
        string userId,
        IEnumerable<UserClaimDto> claims,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var user = await FindUserAsync(userId);
        if (user is null)
        {
            return Result.Fail(
                new AppError(
                    AuthErrorCodes.UserNotFound,
                    "Usuário não encontrado.",
                    ErrorType.NotFound));
        }

        var atuais = await _userManager.GetClaimsAsync(user);

        foreach (var claim in atuais)
        {
            var removeResult = await _userManager.RemoveClaimAsync(user, claim);
            if (!removeResult.Succeeded)
            {
                var errors = removeResult.Errors
                    .Select(e => new AppError(
                        AuthErrorCodes.RemoveClaimFailed,
                        e.Description,
                        ErrorType.Validation))
                    .ToArray();

                return Result.Fail(errors);
            }
        }

        var novasClaims = claims
            .Where(c => !string.IsNullOrWhiteSpace(c.Type) && !string.IsNullOrWhiteSpace(c.Value))
            .DistinctBy(c => new { Tipo = c.Type.ToUpperInvariant(), c.Value })
            .ToList();

        foreach (var claim in novasClaims)
        {
            var addResult = await _userManager.AddClaimAsync(user, new Claim(claim.Type, claim.Value));
            if (!addResult.Succeeded)
            {
                var errors = addResult.Errors
                    .Select(e => new AppError(
                        AuthErrorCodes.AddClaimFailed,
                        e.Description,
                        ErrorType.Validation))
                    .ToArray();

                return Result.Fail(errors);
            }
        }

        return Result.Ok();
    }

    public async Task<Result> RemoveClaimFromUserAsync(
        string userId,
        string claimType,
        string claimValue,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var user = await FindUserAsync(userId);
        if (user is null)
        {
            return Result.Fail(
                new AppError(
                    AuthErrorCodes.UserNotFound,
                    "Usuário não encontrado.",
                    ErrorType.NotFound));
        }

        var claims = await _userManager.GetClaimsAsync(user);

        var claim = claims.FirstOrDefault(c =>
            c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase) &&
            c.Value.Equals(claimValue, StringComparison.Ordinal));

        if (claim is null)
            return Result.Ok();

        var result = await _userManager.RemoveClaimAsync(user, claim);

        if (result.Succeeded)
            return Result.Ok();

        var errors = result.Errors
            .Select(e => new AppError(
                AuthErrorCodes.RemoveClaimFailed,
                e.Description,
                ErrorType.Validation))
            .ToArray();

        return Result.Fail(errors);
    }

    private async Task<ApplicationUser?> FindUserAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<UsuarioListDto?> GetUserByIdAsync(string userId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(userId))
            return null;

        var user = await _userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
            return null;

        return new UsuarioListDto
        {
            Id = user.Id,
            Nome = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            EmailConfirmado = user.EmailConfirmed,
            Ativo = !user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow
        };
    }
}