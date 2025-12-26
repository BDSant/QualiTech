using Microsoft.AspNetCore.Identity;
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

        const string authSchema = "Auth";
        builder.Entity<ApplicationUser>().ToTable("AspNetUsers", authSchema);
        builder.Entity<IdentityRole>().ToTable("AspNetRoles", authSchema);
        builder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles", authSchema);
        builder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims", authSchema);
        builder.Entity<IdentityUserLogin<string>>().ToTable("AspNetUserLogins", authSchema);
        builder.Entity<IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims", authSchema);
        builder.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens", authSchema);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
