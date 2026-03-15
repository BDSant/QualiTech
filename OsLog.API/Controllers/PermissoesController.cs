using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OsLog.Application.Common.Result;
using OsLog.Application.DTOs.Identity;
using OsLog.Application.Ports.Identity.Admin;

namespace OsLog.API.Controllers;

[ApiController]
//[Authorize(Roles = "Master,Admin")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/permissoes")]
public sealed class PermissoesController : BaseApiController
{
    private readonly IIdentityAdminGateway _identityAdminGateway;

    public PermissoesController(IIdentityAdminGateway identityAdminGateway)
    {
        _identityAdminGateway = identityAdminGateway
            ?? throw new ArgumentNullException(nameof(identityAdminGateway));
    }

    [HttpGet("roles")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();

        var roles = await _identityAdminGateway.GetRolesAsync(cancellationToken);
        return Ok(roles);
    }

    [HttpPost("roles")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRole(
        [FromBody] CreateRoleDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var result = await _identityAdminGateway.EnsureRoleExistsAsync(
            request.RoleName.Trim(),
            cancellationToken);

        return CustomResponse(result, StatusCodes.Status201Created);
    }

    [HttpGet("usuarios/{userId}/roles")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserRoles(
        string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("O userId é obrigatório.");

        var roles = await _identityAdminGateway.GetUserRolesAsync(
            userId.Trim(),
            cancellationToken);

        return Ok(roles);
    }

    [HttpPut("usuarios/{userId}/roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReplaceUserRoles(
        string userId,
        [FromBody] ReplaceUserRolesDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("O userId é obrigatório.");

        if (!string.Equals(userId.Trim(), request.UserId?.Trim(), StringComparison.Ordinal))
            return BadRequest("O userId da rota difere do informado no corpo da requisição.");

        var roles = (request.Roles ?? Array.Empty<string>())
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var result = await _identityAdminGateway.ReplaceUserRolesAsync(
            userId.Trim(),
            roles,
            cancellationToken);

        return CustomResponse(result);
    }

    [HttpPost("usuarios/{userId}/roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddUserRole(
        string userId,
        [FromBody] AddUserRoleDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("O userId é obrigatório.");

        if (!string.Equals(userId.Trim(), request.UserId?.Trim(), StringComparison.Ordinal))
            return BadRequest("O userId da rota difere do informado no corpo da requisição.");

        var result = await _identityAdminGateway.AddUserToRoleAsync(
            userId.Trim(),
            request.RoleName.Trim(),
            cancellationToken);

        return CustomResponse(result);
    }

    [HttpDelete("usuarios/{userId}/roles/{roleName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveUserRole(
        string userId,
        string roleName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("O userId é obrigatório.");

        if (string.IsNullOrWhiteSpace(roleName))
            return BadRequest("O roleName é obrigatório.");

        var result = await _identityAdminGateway.RemoveUserFromRoleAsync(
            userId.Trim(),
            roleName.Trim(),
            cancellationToken);

        return CustomResponse(result);
    }

    [HttpGet("usuarios/{userId}/claims")]
    [ProducesResponseType(typeof(UserClaimDto[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserClaims(
        string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("O userId é obrigatório.");

        var claims = await _identityAdminGateway.GetUserClaimsAsync(
            userId.Trim(),
            cancellationToken);

        return Ok(claims);
    }

    [HttpPut("usuarios/{userId}/claims")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReplaceUserClaims(
        string userId,
        [FromBody] ReplaceUserClaimsDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("O userId é obrigatório.");

        if (!string.Equals(userId.Trim(), request.UserId?.Trim(), StringComparison.Ordinal))
            return BadRequest("O userId da rota difere do informado no corpo da requisição.");

        var claims = (request.Claims ?? Array.Empty<UserClaimInputDto>())
            .Where(c => c is not null &&
                        !string.IsNullOrWhiteSpace(c.Type) &&
                        !string.IsNullOrWhiteSpace(c.Value))
            .Select(c => new UserClaimDto
            {
                Type = c.Type.Trim(),
                Value = c.Value.Trim()
            })
            .ToArray();

        var result = await _identityAdminGateway.ReplaceUserClaimsAsync(
            userId.Trim(),
            claims,
            cancellationToken);

        return CustomResponse(result);
    }

    [HttpPost("usuarios/{userId}/claims")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddUserClaim(
        string userId,
        [FromBody] AddUserClaimDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("O userId é obrigatório.");

        if (!string.Equals(userId.Trim(), request.UserId?.Trim(), StringComparison.Ordinal))
            return BadRequest("O userId da rota difere do informado no corpo da requisição.");

        var result = await _identityAdminGateway.AddClaimToUserAsync(
            userId.Trim(),
            request.ClaimType.Trim(),
            request.ClaimValue.Trim(),
            cancellationToken);

        return CustomResponse(result);
    }

    [HttpDelete("usuarios/{userId}/claims")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveUserClaim(
        string userId,
        [FromBody] RemoveUserClaimDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("O userId é obrigatório.");

        if (!string.Equals(userId.Trim(), request.UserId?.Trim(), StringComparison.Ordinal))
            return BadRequest("O userId da rota difere do informado no corpo da requisição.");

        var result = await _identityAdminGateway.RemoveClaimFromUserAsync(
            userId.Trim(),
            request.ClaimType.Trim(),
            request.ClaimValue.Trim(),
            cancellationToken);

        return CustomResponse(result);
    }
}