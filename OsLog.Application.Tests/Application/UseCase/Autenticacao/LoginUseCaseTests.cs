using Moq;
using OsLog.Application.Ports.Identity.Runtime;
using OsLog.Application.Ports.Security;
using OsLog.Application.UseCases.Autenticacao.Common;
using OsLog.Application.UseCases.Autenticacao.Login;
using System.Security.Claims;

namespace OsLog.Tests.Unit.Application.UseCases.Autenticacao;

public class LoginUseCaseTests
{
    [Fact(DisplayName = "[UNIT] Login deve autenticar usuário com vínculo interno e gerar token com contexto")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "LoginUseCase")]
    public async Task ExecuteAsync_DeveAutenticarEGerarTokenComClaimsDeContexto()
    {
        // Arrange
        var identity = new Mock<IIdentityGateway>();
        var jwt = new Mock<IJwtTokenService>();
        var usuarioResolver = new Mock<IUsuarioAutenticadoResolver>();

        var empresaId = Guid.NewGuid();

        var request = new LoginRequest
        {
            Email = "gerente@empresa.com",
            Senha = "Senha@123"
        };

        var user = new IdentityUserDto(
            "user-domain-id-123",
            "gerente@empresa.com");

        var claims = new List<Claim>
        {
            new("scope", "unidade"),
            new("empresa_id", empresaId.ToString()),
            new("unidade_id", "10"),
            new("perfil_acesso", "Gerente")
        };

        var tokenEsperado = new TokenResponse
        {
            AccessToken = "token-valido",
            RefreshToken = "refresh-token",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30),
            RefreshExpiresAtUtc = DateTime.UtcNow.AddDays(7),
            TokenType = "Bearer",
            ExpiresInSeg = 1800
        };

        identity.Setup(x => x.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        identity.Setup(x => x.CheckPasswordAsync(user.Id, request.Senha, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        usuarioResolver.Setup(x => x.ObterUsuarioIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(22);

        identity.Setup(x => x.GetRolesAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "Gerente" });

        identity.Setup(x => x.GetClaimsAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(claims);

        jwt.Setup(x => x.GenerateTokensAsync(
                user.Id,
                user.Email,
                It.IsAny<IEnumerable<string>>(),
                22,
                It.IsAny<IEnumerable<Claim>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokenEsperado);

        var sut = new LoginUseCase(identity.Object, jwt.Object, usuarioResolver.Object);

        // Act
        var result = await sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("token-valido", result.Value!.AccessToken);
        Assert.Equal("Bearer", result.Value.TokenType);

        jwt.Verify(x => x.GenerateTokensAsync(
                user.Id,
                user.Email,
                It.Is<IEnumerable<string>>(r => r.Contains("Gerente")),
                22,
                It.Is<IEnumerable<Claim>>(c =>
                    c.Any(x => x.Type == "scope" && x.Value == "unidade") &&
                    c.Any(x => x.Type == "empresa_id" && x.Value == empresaId.ToString()) &&
                    c.Any(x => x.Type == "unidade_id" && x.Value == "10") &&
                    c.Any(x => x.Type == "perfil_acesso" && x.Value == "Gerente")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}