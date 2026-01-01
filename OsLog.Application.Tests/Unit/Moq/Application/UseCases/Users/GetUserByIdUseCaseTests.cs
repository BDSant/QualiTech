using Moq;
using OsLog.Application.Ports.Identity.Admin;
using OsLog.Application.UseCases.Users;
using System.Security.Claims;

namespace OsLog.Tests.Unit.Moq.Application.UseCases.Users;

public sealed class GetUserByIdUseCaseTests
{
    private readonly Mock<IIdentityAdminGateway> _gateway = new(MockBehavior.Strict);

    private GetUserByIdUseCase CreateSut()
        => new(_gateway.Object);

    [Fact]
    public async Task ExecuteAsync_WhenUserIdIsNullOrWhiteSpace_ShouldReturnValidationError()
    {
        var sut = CreateSut();

        var result = await sut.ExecuteAsync("   ");

        Assert.False(result.Succeeded);
        Assert.False(result.IsNotFound);
        Assert.Null(result.Data);
        Assert.Contains(result.Errors, e => e.Contains("UserId", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        _gateway
            .Setup(x => x.GetUserByIdAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityUserData?)null);

        var sut = CreateSut();

        var result = await sut.ExecuteAsync("1");

        Assert.False(result.Succeeded);
        Assert.True(result.IsNotFound);
        Assert.Null(result.Data);

        _gateway.VerifyAll();
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserExists_ShouldReturnUserDetailsWithRolesAndClaims()
    {
        var user = new IdentityUserData(
            Id: "1",
            UserName: "benne",
            Email: "benne@email.com",
            EmailConfirmed: true);

        _gateway
            .Setup(x => x.GetUserByIdAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _gateway
            .Setup(x => x.GetUserRolesAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityOperationResult<IReadOnlyCollection<string>>.Success(new[] { "Master", "Admin" }));

        _gateway
            .Setup(x => x.GetUserClaimsAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityOperationResult<IReadOnlyCollection<Claim>>.Success(new[]
            {
                new Claim("empresa_id", "10"),
                new Claim("unidade_id", "20")
            }));

        var sut = CreateSut();

        var result = await sut.ExecuteAsync("1");

        Assert.True(result.Succeeded);
        Assert.False(result.IsNotFound);
        Assert.NotNull(result.Data);

        Assert.Equal("1", result.Data!.UserId);
        Assert.Equal("benne", result.Data.UserName);
        Assert.Equal("benne@email.com", result.Data.Email);
        Assert.True(result.Data.EmailConfirmed);

        Assert.Contains("Master", result.Data.Roles);
        Assert.Contains(result.Data.Claims, c => c.Type == "empresa_id" && c.Value == "10");
        Assert.Contains(result.Data.Claims, c => c.Type == "unidade_id" && c.Value == "20");

        _gateway.VerifyAll();
    }

    [Fact]
    public async Task ExecuteAsync_WhenRolesGatewayFails_ShouldFailAndReturnErrors()
    {
        var user = new IdentityUserData("1", "benne", "benne@email.com", true);

        _gateway
            .Setup(x => x.GetUserByIdAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _gateway
            .Setup(x => x.GetUserRolesAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityOperationResult<IReadOnlyCollection<string>>.Failure("Erro ao obter roles"));

        var sut = CreateSut();

        var result = await sut.ExecuteAsync("1");

        Assert.False(result.Succeeded);
        Assert.False(result.IsNotFound);
        Assert.Null(result.Data);
        Assert.Contains("Erro ao obter roles", result.Errors);

        // Como falhou em roles, não deve tentar buscar claims
        _gateway.Verify(x => x.GetUserClaimsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _gateway.VerifyAll();
    }

    [Fact]
    public async Task ExecuteAsync_WhenClaimsGatewayFails_ShouldFailAndReturnErrors()
    {
        var user = new IdentityUserData("1", "benne", "benne@email.com", true);

        _gateway
            .Setup(x => x.GetUserByIdAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _gateway
            .Setup(x => x.GetUserRolesAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityOperationResult<IReadOnlyCollection<string>>.Success(new[] { "Admin" }));

        _gateway
            .Setup(x => x.GetUserClaimsAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityOperationResult<IReadOnlyCollection<Claim>>.Failure("Erro ao obter claims"));

        var sut = CreateSut();

        var result = await sut.ExecuteAsync("1");

        Assert.False(result.Succeeded);
        Assert.False(result.IsNotFound);
        Assert.Null(result.Data);
        Assert.Contains("Erro ao obter claims", result.Errors);

        _gateway.VerifyAll();
    }
}
