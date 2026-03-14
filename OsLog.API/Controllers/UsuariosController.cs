using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OsLog.Application.DTOs.Users;
using OsLog.Application.UseCases.Users;

namespace OsLog.API.Controllers;

[ApiController]
[Authorize(Roles = "Master,Admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/usuarios")]
public sealed class UsuariosController : BaseApiController
{
    private readonly CreateUserUseCase _createUserUseCase;
    private readonly GetUserByIdUseCase _getUserByIdUseCase;

    public UsuariosController(
        CreateUserUseCase createUserUseCase,
        GetUserByIdUseCase getUserByIdUseCase)
    {
        _createUserUseCase = createUserUseCase
            ?? throw new ArgumentNullException(nameof(createUserUseCase));

        _getUserByIdUseCase = getUserByIdUseCase
            ?? throw new ArgumentNullException(nameof(getUserByIdUseCase));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await _getUserByIdUseCase.ExecuteAsync(id, cancellationToken);

        if (result.Succeeded && result.Data is not null)
            return Ok(result.Data);

        if (result.IsNotFound)
            return NotFound();

        return BadRequest(new ProblemDetails
        {
            Title = "Falha ao consultar usuário",
            Detail = "Não foi possível obter o usuário com o id informado.",
            Status = StatusCodes.Status400BadRequest,
            Extensions =
            {
                ["errors"] = result.Errors
            }
        });
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createUserUseCase.ExecuteAsync(request, cancellationToken);

        if (result.Succeeded && result.Data is not null)
        {
            var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    version,
                    id = result.Data.UserId
                },
                result.Data);
        }

        if (result.ErrorCode == CreateUserErrorCode.Conflict)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Conflito ao criar usuário",
                Detail = "Já existe um usuário cadastrado com os dados informados.",
                Status = StatusCodes.Status409Conflict,
                Extensions =
                {
                    ["errors"] = result.Errors,
                    ["errorCode"] = result.ErrorCode.ToString()
                }
            });
        }

        return BadRequest(new ProblemDetails
        {
            Title = "Falha ao criar usuário",
            Detail = "Não foi possível criar o usuário com os dados informados.",
            Status = StatusCodes.Status400BadRequest,
            Extensions =
            {
                ["errors"] = result.Errors,
                ["errorCode"] = result.ErrorCode.ToString()
            }
        });
    }
}