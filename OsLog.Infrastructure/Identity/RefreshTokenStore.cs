using Microsoft.EntityFrameworkCore;
using OsLog.Application.Ports.ApplicationServices;
using OsLog.Infrastructure.EntityFramework;

namespace OsLog.Infrastructure.Identity;

public class RefreshTokenStore : IRefreshTokenStore
{
    private readonly AppDbContext _db;

    public RefreshTokenStore(AppDbContext db) => _db = db;

    public async Task CreateAsync(string userId, string tokenHash, DateTime expiresAtUtc, CancellationToken ct = default)
    {
        var entity = new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAtUtc = expiresAtUtc
        };

        _db.RefreshTokens.Add(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<(Guid Id, string UserId)?> GetValidAsync(string tokenHash, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var token = await _db.RefreshTokens
            .AsNoTracking()
            .Where(x => x.TokenHash == tokenHash && x.RevokedAtUtc == null && x.ExpiresAtUtc > now)
            .Select(x => new { x.Id, x.UserId })
            .FirstOrDefaultAsync(ct);

        return token is null ? null : (token.Id, token.UserId);
    }

    public async Task RevokeAsync(Guid refreshTokenId, string? replacedByTokenHash = null, CancellationToken ct = default)
    {
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.Id == refreshTokenId, ct);
        if (token is null) return;

        if (token.RevokedAtUtc is null)
        {
            token.RevokedAtUtc = DateTime.UtcNow;
            token.ReplacedByTokenHash = replacedByTokenHash;
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task RevokeByHashAsync(string tokenHash, CancellationToken ct = default)
    {
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, ct);
        if (token is null) return;

        if (token.RevokedAtUtc is null)
        {
            token.RevokedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }
}
