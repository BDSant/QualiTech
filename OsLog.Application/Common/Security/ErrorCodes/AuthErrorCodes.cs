namespace OsLog.Application.Common.Security.ErrorCodes;

/// <summary>
/// Códigos de erro estáveis para autenticação/autorização.
/// O front-end deve se basear no Code (e não em Message).
/// </summary>
public static class AuthErrorCodes
{
    public const string EmailRequired = "AUTH_EMAIL_REQUIRED";
    public const string PasswordRequired = "AUTH_PASSWORD_REQUIRED";
    public const string InvalidCredentials = "AUTH_INVALID_CREDENTIALS";

    public const string RefreshTokenRequired = "AUTH_REFRESH_REQUIRED";
    public const string RefreshInvalid = "AUTH_REFRESH_INVALID";
    public const string RefreshExpired = "AUTH_REFRESH_EXPIRED";

    public const string UserNotFound = "AUTH_USER_NOT_FOUND";
    public const string ChangePasswordFailed = "AUTH_CHANGE_PASSWORD_FAILED";
    public const string ResetPasswordFailed = "AUTH_RESET_PASSWORD_FAILED";
}
