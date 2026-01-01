using Microsoft.AspNetCore.Identity;
using OsLog.Application.Ports.Identity.Admin;
using System.Security.Claims;

namespace OsLog.Infrastructure.Identity.Gateway;

/// <summary>
/// Implementação concreta do gateway administrativo de Identity.
/// Aqui é o único lugar onde UserManager/RoleManager devem existir.
/// </summary>
public sealed class IdentityAdminGateway : IIdentityAdminGateway
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public IdentityAdminGateway(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
    }

    public async Task<IdentityUserData?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(email))
            return null;

        var user = await _userManager.FindByEmailAsync(email.Trim());
        return user is null ? null : ToUserData(user);
    }

    public async Task<IdentityUserData?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(userId))
            return null;

        var user = await _userManager.FindByIdAsync(userId.Trim());
        return user is null ? null : ToUserData(user);
    }

    public async Task<IdentityOperationResult<IdentityUserData>> CreateUserAsync(
        string userName,
        string email,
        string password,
        bool emailConfirmed = false,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(userName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return IdentityOperationResult<IdentityUserData>.Failure("UserName, Email e Password são obrigatórios.");
        }

        var user = new ApplicationUser
        {
            UserName = userName.Trim(),
            Email = email.Trim(),
            EmailConfirmed = emailConfirmed
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var code = MapIdentityResultToErrorCode(result);
            return IdentityOperationResult<IdentityUserData>.Failure(code, ToErrors(result));
        }

        return IdentityOperationResult<IdentityUserData>.Success(ToUserData(user));
    }

    public async Task<IdentityOperationResult> EnsureRoleExistsAsync(string roleName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(roleName))
            return IdentityOperationResult.Failure("RoleName é obrigatório.");

        roleName = roleName.Trim();

        if (await _roleManager.RoleExistsAsync(roleName))
            return IdentityOperationResult.Success();

        var create = await _roleManager.CreateAsync(new IdentityRole(roleName));
        return create.Succeeded
            ? IdentityOperationResult.Success()
            : IdentityOperationResult.Failure(ToErrors(create));
    }

    public async Task<IdentityOperationResult> AddUserToRolesAsync(
        string userId,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await FindUserOrFail(userId);
        if (user is null) return IdentityOperationResult.Failure("Usuário não encontrado.");

        var normalizedRoles = NormalizeRoles(roles);
        if (normalizedRoles.Length == 0) return IdentityOperationResult.Success();

        // Idempotência: adicionar apenas roles que o usuário ainda não possui
        var currentRoles = await _userManager.GetRolesAsync(user);
        var toAdd = normalizedRoles
            .Except(currentRoles, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (toAdd.Length == 0)
            return IdentityOperationResult.Success();

        var add = await _userManager.AddToRolesAsync(user, toAdd);
        return add.Succeeded
            ? IdentityOperationResult.Success()
            : IdentityOperationResult.Failure(ToErrors(add));
    }

    public async Task<IdentityOperationResult> ReplaceUserRolesAsync(
        string userId,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await FindUserOrFail(userId);
        if (user is null) return IdentityOperationResult.Failure("Usuário não encontrado.");

        var desired = NormalizeRoles(roles);
        var current = await _userManager.GetRolesAsync(user);

        var toRemove = current.Except(desired, StringComparer.OrdinalIgnoreCase).ToArray();
        var toAdd = desired.Except(current, StringComparer.OrdinalIgnoreCase).ToArray();

        if (toRemove.Length > 0)
        {
            var rem = await _userManager.RemoveFromRolesAsync(user, toRemove);
            if (!rem.Succeeded) return IdentityOperationResult.Failure(ToErrors(rem));
        }

        if (toAdd.Length > 0)
        {
            var add = await _userManager.AddToRolesAsync(user, toAdd);
            if (!add.Succeeded) return IdentityOperationResult.Failure(ToErrors(add));
        }

        return IdentityOperationResult.Success();
    }

    public async Task<IdentityOperationResult> AddClaimsAsync(
        string userId,
        IEnumerable<Claim> claims,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await FindUserOrFail(userId);
        if (user is null) return IdentityOperationResult.Failure("Usuário não encontrado.");

        var normalizedClaims = NormalizeClaims(claims);
        if (normalizedClaims.Length == 0) return IdentityOperationResult.Success();

        // Idempotência: adicionar apenas claims que ainda não existem
        var existing = await _userManager.GetClaimsAsync(user);
        var toAdd = normalizedClaims
            .Where(c => !existing.Any(e =>
                string.Equals(e.Type, c.Type, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(e.Value, c.Value, StringComparison.Ordinal)))
            .ToArray();

        if (toAdd.Length == 0)
            return IdentityOperationResult.Success();

        var add = await _userManager.AddClaimsAsync(user, toAdd);
        return add.Succeeded
            ? IdentityOperationResult.Success()
            : IdentityOperationResult.Failure(ToErrors(add));
    }

    public async Task<IdentityOperationResult> ReplaceClaimsAsync(
        string userId,
        IEnumerable<Claim> claims,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await FindUserOrFail(userId);
        if (user is null) return IdentityOperationResult.Failure("Usuário não encontrado.");

        var desired = NormalizeClaims(claims);
        if (desired.Length == 0) return IdentityOperationResult.Success();

        // Estratégia: para os tipos presentes em "desired", remove todas as claims atuais desses tipos e insere as novas.
        var desiredTypes = desired
            .Select(c => c.Type)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var existing = await _userManager.GetClaimsAsync(user);
        var toRemove = existing
            .Where(c => desiredTypes.Contains(c.Type, StringComparer.OrdinalIgnoreCase))
            .ToArray();

        if (toRemove.Length > 0)
        {
            var rem = await _userManager.RemoveClaimsAsync(user, toRemove);
            if (!rem.Succeeded) return IdentityOperationResult.Failure(ToErrors(rem));
        }

        var add = await _userManager.AddClaimsAsync(user, desired);
        return add.Succeeded
            ? IdentityOperationResult.Success()
            : IdentityOperationResult.Failure(ToErrors(add));
    }

    public async Task<IdentityOperationResult> RemoveClaimsAsync(
        string userId,
        IEnumerable<Claim> claims,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await FindUserOrFail(userId);
        if (user is null) return IdentityOperationResult.Failure("Usuário não encontrado.");

        var normalized = NormalizeClaims(claims);
        if (normalized.Length == 0) return IdentityOperationResult.Success();

        // Idempotência: remover apenas o que existe
        var existing = await _userManager.GetClaimsAsync(user);
        var toRemove = normalized
            .Where(c => existing.Any(e =>
                string.Equals(e.Type, c.Type, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(e.Value, c.Value, StringComparison.Ordinal)))
            .ToArray();

        if (toRemove.Length == 0)
            return IdentityOperationResult.Success();

        var rem = await _userManager.RemoveClaimsAsync(user, toRemove);
        return rem.Succeeded
            ? IdentityOperationResult.Success()
            : IdentityOperationResult.Failure(ToErrors(rem));
    }

    public async Task<IdentityOperationResult<IReadOnlyCollection<string>>> GetUserRolesAsync(
    string userId,
    CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await FindUserOrFail(userId);
        if (user is null)
            return IdentityOperationResult<IReadOnlyCollection<string>>.Failure("Usuário não encontrado.");

        var roles = await _userManager.GetRolesAsync(user);
        return IdentityOperationResult<IReadOnlyCollection<string>>.Success(roles.ToArray());
    }

    public async Task<IdentityOperationResult<IReadOnlyCollection<Claim>>> GetUserClaimsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await FindUserOrFail(userId);
        if (user is null)
            return IdentityOperationResult<IReadOnlyCollection<Claim>>.Failure("Usuário não encontrado.");

        var claims = await _userManager.GetClaimsAsync(user);
        return IdentityOperationResult<IReadOnlyCollection<Claim>>.Success(claims.ToArray());
    }

    // ------------------------
    // Helpers
    // ------------------------

    private async Task<ApplicationUser?> FindUserOrFail(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        return await _userManager.FindByIdAsync(userId.Trim());
    }

    private static IdentityUserData ToUserData(ApplicationUser user) =>
        new(
            Id: user.Id,
            UserName: user.UserName ?? string.Empty,
            Email: user.Email ?? string.Empty,
            EmailConfirmed: user.EmailConfirmed
        );

    private static string[] ToErrors(IdentityResult result) =>
        result.Errors?.Select(e => e.Description).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray()
        ?? Array.Empty<string>();

    private static string[] NormalizeRoles(IEnumerable<string> roles) =>
        (roles ?? Array.Empty<string>())
        .Where(r => !string.IsNullOrWhiteSpace(r))
        .Select(r => r.Trim())
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

    private static Claim[] NormalizeClaims(IEnumerable<Claim> claims) =>
        (claims ?? Array.Empty<Claim>())
        .Where(c => c is not null && !string.IsNullOrWhiteSpace(c.Type))
        .Select(c => new Claim(c.Type.Trim(), (c.Value ?? string.Empty).Trim()))
        .Distinct(new ClaimTypeValueComparer())
        .ToArray();

    private sealed class ClaimTypeValueComparer : IEqualityComparer<Claim>
    {
        public bool Equals(Claim? x, Claim? y)
        {
            if (x is null && y is null) return true;
            if (x is null || y is null) return false;

            return string.Equals(x.Type, y.Type, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(x.Value, y.Value, StringComparison.Ordinal);
        }

        public int GetHashCode(Claim obj)
        {
            unchecked
            {
                var h1 = StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Type ?? string.Empty);
                var h2 = StringComparer.Ordinal.GetHashCode(obj.Value ?? string.Empty);
                return (h1 * 397) ^ h2;
            }
        }
    }

    private static IdentityOperationErrorCode MapIdentityResultToErrorCode(IdentityResult result)
    {
        // Códigos padrão do ASP.NET Identity:
        // - DuplicateEmail
        // - DuplicateUserName
        var hasDuplicate =
            result.Errors?.Any(e =>
                string.Equals(e.Code, "DuplicateEmail", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(e.Code, "DuplicateUserName", StringComparison.OrdinalIgnoreCase)) == true;

        return hasDuplicate
            ? IdentityOperationErrorCode.Conflict
            : IdentityOperationErrorCode.IdentityError;
    }

}

