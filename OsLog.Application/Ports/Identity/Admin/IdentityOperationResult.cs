namespace OsLog.Application.Ports.Identity.Admin;

/// <summary>
/// Resultado base (não genérico) para operações do gateway de Identity.
/// </summary>
public class IdentityOperationResult
{
    public bool Succeeded { get; init; }

    public IdentityOperationErrorCode ErrorCode { get; init; } = IdentityOperationErrorCode.None;

    public IReadOnlyCollection<string> Errors { get; init; } = Array.Empty<string>();

    public static IdentityOperationResult Success() =>
        new() { Succeeded = true, ErrorCode = IdentityOperationErrorCode.None };

    public static IdentityOperationResult Failure(params string[] errors) =>
        Failure(IdentityOperationErrorCode.IdentityError, errors);

    public static IdentityOperationResult Failure(IEnumerable<string> errors) =>
        Failure(IdentityOperationErrorCode.IdentityError, errors);

    public static IdentityOperationResult Failure(IdentityOperationErrorCode code, params string[] errors) =>
        new()
        {
            Succeeded = false,
            ErrorCode = code,
            Errors = (errors is { Length: > 0 }) ? errors : new[] { "Unknown error." }
        };

    public static IdentityOperationResult Failure(IdentityOperationErrorCode code, IEnumerable<string> errors) =>
        new()
        {
            Succeeded = false,
            ErrorCode = code,
            Errors = (errors ?? Array.Empty<string>()).ToArray()
        };
}

/// <summary>
/// Resultado genérico para operações do gateway de Identity.
/// Observação: "new" nos métodos Failure elimina warning CS0108 (hiding inherited member).
/// </summary>
public sealed class IdentityOperationResult<T> : IdentityOperationResult
{
    public T? Data { get; init; }

    public static IdentityOperationResult<T> Success(T data) =>
        new() { Succeeded = true, ErrorCode = IdentityOperationErrorCode.None, Data = data };

    public static new IdentityOperationResult<T> Failure(params string[] errors) =>
        Failure(IdentityOperationErrorCode.IdentityError, errors);

    public static new IdentityOperationResult<T> Failure(IEnumerable<string> errors) =>
        Failure(IdentityOperationErrorCode.IdentityError, errors);

    public static new IdentityOperationResult<T> Failure(IdentityOperationErrorCode code, params string[] errors) =>
        new()
        {
            Succeeded = false,
            ErrorCode = code,
            Errors = (errors is { Length: > 0 }) ? errors : new[] { "Unknown error." }
        };

    public static new IdentityOperationResult<T> Failure(IdentityOperationErrorCode code, IEnumerable<string> errors) =>
        new()
        {
            Succeeded = false,
            ErrorCode = code,
            Errors = (errors ?? Array.Empty<string>()).ToArray()
        };
}
