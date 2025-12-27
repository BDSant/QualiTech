using System;
using System.Collections.Generic;
using System.Text;

namespace OsLog.Application.Common.Result;

public enum ErrorType
{
    Validation,
    Unauthorized,
    Forbidden,
    NotFound,
    Conflict,
    BusinessRule,
    Unexpected
}


