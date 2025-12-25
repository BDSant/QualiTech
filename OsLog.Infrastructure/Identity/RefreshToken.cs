using System;

namespace OsLog.Infrastructure.Identity;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string UserId { get; set; } = string.Empty;

    // Armazene o HASH (não o token puro)
    public string TokenHash { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }
    public string? ReplacedByTokenHash { get; set; }

    public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;

    public ApplicationUser User { get; set; } = null!;
}
