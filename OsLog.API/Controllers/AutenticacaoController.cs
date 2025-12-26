using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OsLog.Api.DTOs.Auth;
using OsLog.API.Services.Auth;
using OsLog.Application.DTOs.Auth;
using OsLog.Infrastructure.Identity;

namespace OsLog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AutenticacaoController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AutenticacaoController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Unauthorized("Credenciais inválidas.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Senha, lockoutOnFailure: true);
        if (!result.Succeeded)
            return Unauthorized("Credenciais inválidas.");

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);
        var token = await _jwtTokenService.GenerateTokensAsync(
            user.Id,
            user.Email ?? request.Email,
            roles,
            claims,
            ct);

        return Ok(token);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponseDto>> Refresh([FromBody] RefreshRequestDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest("RefreshToken é obrigatório.");

        try
        {
            var token = await _jwtTokenService.RefreshAsync(request.RefreshToken, ct);
            return Ok(token);
        }
        catch
        {
            return Unauthorized("Refresh token inválido ou expirado.");
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequestDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest("RefreshToken é obrigatório.");

        await _jwtTokenService.LogoutAsync(request.RefreshToken, ct);
        return NoContent();
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return NotFound("Usuário não encontrado.");

        var result = await _userManager.ChangePasswordAsync(user, request.SenhaAtual, request.NovaSenha);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { errors });
        }

        return NoContent();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return NotFound("Usuário não encontrado.");

        // Gera token de reset e usa para redefinir a senha sem conhecer a antiga
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NovaSenha);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { errors });
        }

        return NoContent();
    }
}
