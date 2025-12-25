namespace OsLog.Application.DTOs.Auth;

public class TokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }

    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshExpiresAtUtc { get; set; }

    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }          // em segundos
    public DateTime ExpiresAt { get; set; }     // UTC
}
