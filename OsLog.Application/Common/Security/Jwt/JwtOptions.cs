namespace OsLog.Application.Common.Security.Jwt;

/// <summary>
/// Configurações de JWT utilizadas pela infraestrutura de emissão/validação.
/// </summary>
public sealed class JwtOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Mantido por compatibilidade com appsettings, mesmo que as chaves de assinatura
    /// sejam geridas via JWKS (NetDevPack).
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 30;
    public int RefreshTokenDays { get; set; } = 7;
}
