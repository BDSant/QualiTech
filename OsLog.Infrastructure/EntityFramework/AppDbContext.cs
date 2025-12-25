using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetDevPack.Security.Jwt.Core.Model;
using NetDevPack.Security.Jwt.Store.EntityFrameworkCore;
using OsLog.Domain.Entities;
using OsLog.Infrastructure.Identity;

namespace OsLog.Infrastructure.EntityFramework;

public class AppDbContext : IdentityDbContext<ApplicationUser>, ISecurityKeyContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Empresa> Empresas { get; set; } = null!;

    // Implementação correta da interface (tipo e nome exatamente como exigido)
    public DbSet<KeyMaterial> SecurityKeys { get; set; } = null!;

    // Seus Refresh Tokens
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
