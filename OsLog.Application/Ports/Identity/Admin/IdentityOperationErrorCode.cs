namespace OsLog.Application.Ports.Identity.Admin;

/// <summary>
/// Códigos determinísticos para padronizar falhas retornadas pelo Gateway.
/// </summary>
public enum IdentityOperationErrorCode
{
    None = 0,
    Conflict = 1,
    NotFound = 2,
    Validation = 3,
    IdentityError = 4
}
