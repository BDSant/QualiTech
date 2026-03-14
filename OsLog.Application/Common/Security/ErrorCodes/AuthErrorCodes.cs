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
    public const string UserAlreadyExists = "AUTH_USER_ALREADY_EXISTS";
    public const string CreateUserFailed = "AUTH_CREATE_USER_FAILED";
    public const string InvalidRole = "AUTH_INVALID_ROLE";
    public const string RoleNotFound = "AUTH_ROLE_NOT_FOUND";
    public const string CreateRoleFailed = "AUTH_CREATE_ROLE_FAILED";
    public const string AddRoleFailed = "AUTH_ADD_ROLE_FAILED";
    public const string RemoveRoleFailed = "AUTH_REMOVE_ROLE_FAILED";
    public const string AddClaimFailed = "AUTH_ADD_CLAIM_FAILED";
    public const string RemoveClaimFailed = "AUTH_REMOVE_CLAIM_FAILED";
}
