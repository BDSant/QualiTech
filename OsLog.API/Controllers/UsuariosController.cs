using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OsLog.Application.DTOs.Users;
using OsLog.Application.UseCases.Users;

namespace OsLog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Master,Admin")]
public sealed class UsuariosController : ControllerBase
{
    private readonly CreateUserUseCase _createUserUseCase;
    private readonly GetUserByIdUseCase _getUserByIdUseCase;

    public UsuariosController(
        CreateUserUseCase createUserUseCase,
        GetUserByIdUseCase getUserByIdUseCase)
    {
        _createUserUseCase = createUserUseCase ?? throw new ArgumentNullException(nameof(createUserUseCase));
        _getUserByIdUseCase = getUserByIdUseCase ?? throw new ArgumentNullException(nameof(getUserByIdUseCase));
    }

    // GET api/usuarios/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById([FromRoute] string id, CancellationToken cancellationToken)
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
            Extensions = { ["errors"] = result.Errors }
        });
    }

    // POST api/usuarios
    [HttpPost]
    [ProducesResponseType(typeof(CreateUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _createUserUseCase.ExecuteAsync(request, cancellationToken);

        if (result.Succeeded && result.Data is not null)
        {
            return CreatedAtAction(
                actionName: nameof(GetById),
                routeValues: new { id = result.Data.UserId },
                value: result.Data);
        }

        if (result.ErrorCode == CreateUserErrorCode.Conflict)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Conflito ao criar usuário",
                Detail = "Já existe um usuário cadastrado com os dados informados.",
                Status = StatusCodes.Status409Conflict,
                Extensions = { ["errors"] = result.Errors, ["errorCode"] = result.ErrorCode.ToString() }
            });
        }

        // Validation e IdentityError -> 400 (mínimo refactor)
        return BadRequest(new ProblemDetails
        {
            Title = "Falha ao criar usuário",
            Detail = "Não foi possível criar o usuário com os dados informados.",
            Status = StatusCodes.Status400BadRequest,
            Extensions = { ["errors"] = result.Errors, ["errorCode"] = result.ErrorCode.ToString() }
        });
    }
}
