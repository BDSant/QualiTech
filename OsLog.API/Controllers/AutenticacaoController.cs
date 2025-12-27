using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OsLog.Application.DTOs.Auth;
using OsLog.Application.UseCases.Autenticacao.ChangePassword;
using OsLog.Application.UseCases.Autenticacao.Login;
using OsLog.Application.UseCases.Autenticacao.Logout;
using OsLog.Application.UseCases.Autenticacao.RefreshToken;
using OsLog.Application.UseCases.Autenticacao.ResetPassword;

namespace OsLog.Api.Controllers;

[Route("api/[controller]")]
public class AutenticacaoController : BaseApiController
{
    private readonly ILoginUseCase _login;
    private readonly IRefreshTokenUseCase _refresh;
    private readonly ILogoutUseCase _logout;
    private readonly IChangePasswordUseCase _changePassword;
    private readonly IResetPasswordUseCase _resetPassword;

    public AutenticacaoController(
        ILoginUseCase login,
        IRefreshTokenUseCase refresh,
        ILogoutUseCase logout,
        IChangePasswordUseCase changePassword,
        IResetPasswordUseCase resetPassword)
    {
        _login = login;
        _refresh = refresh;
        _logout = logout;
        _changePassword = changePassword;
        _resetPassword = resetPassword;
    }

    [HttpPost("login"), AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _login.ExecuteAsync(request, ct);
        return CustomResponse(result, successStatus: 200);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _refresh.ExecuteAsync(request, ct);
        return CustomResponse(result, successStatus: 200);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        var result = await _logout.ExecuteAsync(request, ct);
        return CustomResponse(result, successStatus: 204);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var result = await _changePassword.ExecuteAsync(request, ct);
        return CustomResponse(result, successStatus: 204);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var result = await _resetPassword.ExecuteAsync(request, ct);
        return CustomResponse(result, successStatus: 204);
    }
}
