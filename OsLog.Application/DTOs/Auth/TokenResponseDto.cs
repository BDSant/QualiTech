namespace OsLog.Api.DTOs.Auth;

public class TokenResponseDto
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }          // em segundos
    public DateTime ExpiresAt { get; set; }     // UTC
}
