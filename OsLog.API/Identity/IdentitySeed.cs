using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OsLog.Infrastructure.EntityFramework;
using OsLog.Infrastructure.Identity;

namespace OsLog.Api.Identity;

public static class IdentitySeed
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // 1) Garante que as roles existem (incluindo Master)
        var roles = new[]
        {
            "Master",
            "Admin",
            "GerenteFinanceiro",
            "Atendente",
            "Tecnico"
        };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                    throw new Exception($"Erro ao criar role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        // 2) Cria o usuário Master, se ainda não existir
        const string masterEmail = "master@oslog.local";
        const string masterPassword = "Master@123"; // depois troca isso em produção 😉

        var masterUser = await userManager.FindByEmailAsync(masterEmail);
        if (masterUser == null)
        {
            masterUser = new ApplicationUser
            {
                UserName = masterEmail,
                Email = masterEmail,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(masterUser, masterPassword);
            if (!createResult.Succeeded)
            {
                throw new Exception($"Erro ao criar usuário Master: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }

            // coloca na role Master
            var addRoleResult = await userManager.AddToRoleAsync(masterUser, "Master");
            if (!addRoleResult.Succeeded)
            {
                throw new Exception($"Erro ao atribuir role Master: {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
            }

            // Claims básicas pro Master (opcional)
            await userManager.AddClaimsAsync(masterUser, new[]
            {
                new Claim("nome", "Usuário Master"),
                new Claim("tipo_usuario", "master")
            });
        }

        // Aqui, se você quiser, pode também criar uma empresa DEMO e vincular o Master,
        // mas como o Master enxerga tudo, nem é obrigatório ter registros em UsuarioAcesso.
    }
}
