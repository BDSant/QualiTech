using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OsLog.Application.Common.Security;
using OsLog.Domain.Entities;
using OsLog.Domain.Enums;
using OsLog.Infrastructure.EntityFramework;
using System.Security.Claims;

namespace OsLog.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await EnsureRoleExistsAsync(roleManager, "Master");

        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var adminEmail = config["IdentitySeed:MasterEmail"] ?? "master@oslog.local";
        var adminPassword = config["IdentitySeed:MasterPassword"] ?? "Master@123456";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(" | ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Erro ao criar usuário administrador: {errors}");
            }
        }

        await EnsureUserInRoleAsync(userManager, adminUser, "Master");

        await EnsureClaimAsync(userManager, adminUser, "permissao", Permissions.Empresa.Criar);
        await EnsureClaimAsync(userManager, adminUser, "permissao", Permissions.Empresa.Consultar);
        await EnsureClaimAsync(userManager, adminUser, "permissao", Permissions.Empresa.Excluir);

        await EnsureUsuarioAcessoPlataformaAsync(dbContext, adminUser.Id);
    }

    private static async Task EnsureRoleExistsAsync(
        RoleManager<IdentityRole> roleManager,
        string roleName)
    {
        var exists = await roleManager.RoleExistsAsync(roleName);
        if (exists)
            return;

        var result = await roleManager.CreateAsync(new IdentityRole(roleName));
        if (!result.Succeeded)
        {
            var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Erro ao criar role '{roleName}': {errors}");
        }
    }

    private static async Task EnsureUserInRoleAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user,
        string roleName)
    {
        var isInRole = await userManager.IsInRoleAsync(user, roleName);
        if (isInRole)
            return;

        var result = await userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(
                $"Erro ao adicionar usuário '{user.Email}' à role '{roleName}': {errors}");
        }
    }

    private static async Task EnsureClaimAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user,
        string claimType,
        string claimValue)
    {
        var claims = await userManager.GetClaimsAsync(user);

        var exists = claims.Any(c =>
            c.Type == claimType &&
            c.Value == claimValue);

        if (exists)
            return;

        var result = await userManager.AddClaimAsync(user, new Claim(claimType, claimValue));
        if (!result.Succeeded)
        {
            var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(
                $"Erro ao adicionar claim '{claimType}={claimValue}' ao usuário '{user.Email}': {errors}");
        }
    }

    private static async Task EnsureUsuarioAcessoPlataformaAsync(
        AppDbContext dbContext,
        string usuarioId)
    {
        var exists = await dbContext.UsuariosAcessos
            .AsNoTracking()
            .AnyAsync(x => x.UsuarioId == usuarioId && x.Ativo);

        if (exists)
            return;

        var usuarioAcesso = new UsuarioAcesso
        {
            UsuarioId = usuarioId,
            EmpresaId = null,
            UnidadeId = null,
            Escopo = EscopoAcesso.Plataforma,
            Perfil = PerfilAcesso.Administrador,
            Ativo = true,
            DataCriacaoUtc = DateTime.UtcNow
        };

        await dbContext.UsuariosAcessos.AddAsync(usuarioAcesso);
        await dbContext.SaveChangesAsync();
    }
}