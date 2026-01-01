using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using OsLog.Application.DTOs.Auth;
using OsLog.Application.DTOs.Users;
using OsLog.Application.UseCases.Autenticacao.Login;
using OsLog.Tests.Integration;
using Xunit;

namespace OsLog.Tests.Integration.Api;

public sealed class UsuariosControllerIntegrationTests : IClassFixture<OsLogApiFactory>
{
    private readonly HttpClient _client;

    public UsuariosControllerIntegrationTests(OsLogApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ---------------------------
    // Helpers
    // ---------------------------

    private async Task<string> LoginAsMasterAndGetAccessTokenAsync()
    {
        var request = new LoginRequest
        {
            Email = "master@oslog.local",
            Senha = "Master@123456"
        };

        var response = await _client.PostAsJsonAsync("/api/autenticacao/login", request);
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
        Assert.NotNull(token);
        Assert.False(string.IsNullOrWhiteSpace(token!.AccessToken));

        return token.AccessToken;
    }

    private void SetBearer(string accessToken)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
    }

    private static string UniqueEmail(string prefix = "user")
        => $"{prefix}.{Guid.NewGuid():N}@oslog.local";

    // ---------------------------
    // Tests
    // ---------------------------

    [Fact(DisplayName = "[INT] POST /api/usuarios + GET /api/usuarios/{id} - deve criar e consultar usuário")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "UsuariosController")]
    public async Task Create_Then_GetById_ShouldWork()
    {
        // Arrange
        var token = await LoginAsMasterAndGetAccessTokenAsync();
        SetBearer(token);

        var email = UniqueEmail("int");
        var createReq = new CreateUserRequest
        {
            UserName = email,
            Email = email,
            Password = "User@1234",
            ConfirmPassword = "User@1234",
            EmailConfirmed = true,
            Roles = new[] { "Admin" },
            Claims = new[]
            {
                new ClaimDto { Type = "empresa_id", Value = "1" },
                new ClaimDto { Type = "unidade_id", Value = "1" }
            }
        };

        // Act 1: Create
        var createResp = await _client.PostAsJsonAsync("/api/usuarios", createReq);

        // Assert 1
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

        var created = await createResp.Content.ReadFromJsonAsync<CreateUserResponse>();
        Assert.NotNull(created);
        Assert.False(string.IsNullOrWhiteSpace(created!.UserId));

        // Act 2: GetById (via Location ou rota direta)
        var getUrl = createResp.Headers.Location?.ToString() ?? $"/api/usuarios/{created.UserId}";
        var getResp = await _client.GetAsync(getUrl);

        // Assert 2
        Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);

        var details = await getResp.Content.ReadFromJsonAsync<UserDetailsResponse>();
        Assert.NotNull(details);

        Assert.Equal(created.UserId, details!.UserId);
        Assert.Equal(email, details.Email);

        Assert.Contains("Admin", details.Roles);

        Assert.Contains(details.Claims, c => c.Type == "empresa_id" && c.Value == "1");
        Assert.Contains(details.Claims, c => c.Type == "unidade_id" && c.Value == "1");
    }

    [Fact(DisplayName = "[INT] POST /api/usuarios - deve retornar 409 quando e-mail já existe")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "UsuariosController")]
    public async Task Create_WhenEmailAlreadyExists_ShouldReturn409()
    {
        // Arrange
        var token = await LoginAsMasterAndGetAccessTokenAsync();
        SetBearer(token);

        var email = UniqueEmail("conflict");

        var req = new CreateUserRequest
        {
            UserName = email,
            Email = email,
            Password = "User@1234",
            ConfirmPassword = "User@1234",
            EmailConfirmed = true
        };

        // Act 1: primeira criação
        var first = await _client.PostAsJsonAsync("/api/usuarios", req);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        // Act 2: segunda tentativa com mesmo e-mail
        var second = await _client.PostAsJsonAsync("/api/usuarios", req);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);

        // Opcional: validar que veio ProblemDetails (ou pelo menos JSON)
        var json = await second.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(json));

        // Se você padronizou ProblemDetails com Extensions["errorCode"], dá para checar:
        // (Sem travar o teste caso formato mude; por isso, parsing defensivo)
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("status", out var status))
                Assert.Equal(409, status.GetInt32());
        }
        catch
        {
            // ignora: formato pode não ser ProblemDetails dependendo do pipeline
        }
    }
}
