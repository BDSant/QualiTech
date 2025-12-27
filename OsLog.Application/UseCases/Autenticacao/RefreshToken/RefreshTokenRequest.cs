namespace OsLog.Application.UseCases.Autenticacao.RefreshToken;

public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
