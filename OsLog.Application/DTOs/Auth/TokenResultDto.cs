namespace OsLog.Application.DTOs.Auth;

public class TokenResultDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiraEm { get; set; }

    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiraEm { get; set; }
}
