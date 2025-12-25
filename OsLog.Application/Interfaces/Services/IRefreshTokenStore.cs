namespace OsLog.Application.Interfaces.Services;

public record RefreshTokenRecord(
    Guid Id,
    string UserId,
    DateTime ExpiresAtUtc,
    DateTime? RevokedAtUtc
);

public interface IRefreshTokenStore
{
    Task CreateAsync(string userId, string tokenHash, DateTime expiresAtUtc, CancellationToken ct = default);
    Task<(Guid Id, string UserId)?> GetValidAsync(string tokenHash, CancellationToken ct = default);
    Task RevokeAsync(Guid refreshTokenId, string? replacedByTokenHash = null, CancellationToken ct = default);
    Task RevokeByHashAsync(string tokenHash, CancellationToken ct = default);
}
