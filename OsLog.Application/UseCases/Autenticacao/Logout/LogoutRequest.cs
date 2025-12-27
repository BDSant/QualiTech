namespace OsLog.Application.UseCases.Autenticacao.Logout;

public sealed class LogoutRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
