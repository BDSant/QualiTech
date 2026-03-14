using Microsoft.AspNetCore.Mvc;
using OsLog.Application.Common.Result;
using System.Security.Claims;

namespace OsLog.API.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult CustomResponse(Result result, int successStatus = 200)
    {
        if (result.IsSuccess)
            return StatusCode(successStatus);

        return BuildErrorResponse(result.Errors);
    }

    protected IActionResult CustomResponse<T>(Result<T> result, int successStatus = 200)
    {
        if (result.IsSuccess)
            return StatusCode(successStatus, result.Value);

        return BuildErrorResponse(result.Errors);
    }

    protected int? ObterUsuarioId()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

        var claimValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub") ??
            User.FindFirstValue("userId") ??
            User.FindFirstValue("userid");

        return int.TryParse(claimValue, out var usuarioId)
            ? usuarioId
            : null;
    }

    private IActionResult BuildErrorResponse(IReadOnlyList<AppError> errors)
    {
        var status = errors.Select(e => e.ResolveHttpStatus()).DefaultIfEmpty(400).Max();

        var payload = new
        {
            success = false,
            errors = errors.Select(e => new
            {
                code = e.Code,
                message = e.Message,
                type = e.Type.ToString(),
                field = e.Field,
                metadata = e.Metadata
            })
        };

        return StatusCode(status, payload);
    }
}
