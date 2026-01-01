using Microsoft.AspNetCore.Identity;
using OsLog.Infrastructure.Identity;

namespace OsLog.API.Identity;

public static class IdentitySeed
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Roles
        var roles = new[]
        {
            "Master",
            "Admin",
            "GerenteFinanceiro",
            "Atendente",
            "Tecnico"
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Master (via config/env)
        var masterEmail = config["IdentitySeed:MasterEmail"] ?? "master@oslog.local";
        var masterPassword = config["IdentitySeed:MasterPassword"] ?? "Master@123456";

        var masterUser = await userManager.FindByEmailAsync(masterEmail);
        if (masterUser is null)
        {
            masterUser = new ApplicationUser
            {
                UserName = masterEmail,
                Email = masterEmail,
                EmailConfirmed = true
            };

            var create = await userManager.CreateAsync(masterUser, masterPassword);
            if (!create.Succeeded) return;
        }

        if (!await userManager.IsInRoleAsync(masterUser, "Master"))
            await userManager.AddToRoleAsync(masterUser, "Master");

        // Claims mínimas para “super visão” (exemplo)
        var existingClaims = await userManager.GetClaimsAsync(masterUser);
        if (!existingClaims.Any(c => c.Type == "scope" && c.Value == "global"))
            await userManager.AddClaimAsync(masterUser, new System.Security.Claims.Claim("scope", "global"));
    }
}
