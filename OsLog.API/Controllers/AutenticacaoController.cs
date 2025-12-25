using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OsLog.Api.DTOs.Auth;
using OsLog.API.Services.Auth;
using OsLog.Infrastructure.Identity;

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
    public async Task<IActionResult> Login([FromBody] LoginRequestDto model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return Unauthorized();

        var result = await _signInManager.CheckPasswordSignInAsync(
            user, model.Senha, lockoutOnFailure: true);

        if (!result.Succeeded)
            return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);

        var tokenResponse = await _jwtTokenService.GenerateTokensAsync(
            user.Id,
            user.Email,
            user.UserName,
            roles,
            ct: ct);

        return Ok(tokenResponse);
    }
}
