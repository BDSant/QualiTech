using Microsoft.AspNetCore.Authorization;

namespace OsLog.API.Authorization;

public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User?.Identity is null || !context.User.Identity.IsAuthenticated)
            return Task.CompletedTask;

        var hasPermission = context.User.Claims.Any(c =>
            c.Type == "permissao" &&
            c.Value == requirement.Permission);

        if (hasPermission)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}