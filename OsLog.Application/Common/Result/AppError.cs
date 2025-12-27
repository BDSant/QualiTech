using System;
using System.Collections.Generic;
using System.Text;

namespace OsLog.Application.Common.Result;


public sealed record AppError(
    string Code,
    string Message,
    ErrorType Type = ErrorType.BusinessRule,
    string? Field = null,
    int? HttpStatus = null,
    object? Metadata = null
)
{
    public int ResolveHttpStatus() => HttpStatus ?? Type switch
    {
        ErrorType.Validation => 422,
        ErrorType.Unauthorized => 401,
        ErrorType.Forbidden => 403,
        ErrorType.NotFound => 404,
        ErrorType.Conflict => 409,
        ErrorType.BusinessRule => 400,
        _ => 500
    };
}