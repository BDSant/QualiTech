using OsLog.Application.DTOs.Auth;
using OsLog.Application.DTOs.Users;
using OsLog.Application.UseCases.Autenticacao.Login;
using OsLog.Tests.Shared.Factories;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace OsLog.Tests.Integration.Api.Controllers;

public sealed class UsuariosControllerIntegrationTests : IClassFixture<OsLogApiFactory>
{
    private readonly OsLogApiFactory _factory;

    public UsuariosControllerIntegrationTests(OsLogApiFactory factory)
    {
        _factory = factory;
    }

    // ---------------------------
    // Helpers
    // ---------------------------

    private HttpClient CreateClient() => _factory.CreateClient();

    private static async Task<string> LoginAsync(HttpClient client, string email, string senha)
    {
        var response = await client.PostAsJsonAsync("/api/autenticacao/login", new LoginRequest
        {
            Email = email,
            Senha = senha
        });

        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
        Assert.NotNull(token);
        Assert.False(string.IsNullOrWhiteSpace(token!.AccessToken));

        return token.AccessToken!;
    }

    private static Task<string> LoginAsPrivilegedAsync(HttpClient client)
        => LoginAsync(client, "admin2@oslog.local", "Admin@123");

    private static void SetBearer(HttpClient client, string accessToken)
        => client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    private static string UniqueEmail(string prefix)
        => $"{prefix}.{Guid.NewGuid():N}@oslog.local";

    private static CreateUserRequest CreateValidRequest(
        string email,
        string userName,
        string password = "User@1234",
        IEnumerable<string>? roles = null,
        IEnumerable<ClaimDto>? claims = null)
    {
        return new CreateUserRequest
        {
            UserName = userName,
            Email = email,
            Password = password,
            ConfirmPassword = password,
            EmailConfirmed = true,
            Roles = (roles ?? Array.Empty<string>()).ToArray(),
            Claims = (claims ?? Array.Empty<ClaimDto>()).ToArray()
        };
    }

    private static async Task<(CreateUserResponse Created, string? Location)> CreateUserAsync(HttpClient client, CreateUserRequest request)
    {
        var response = await client.PostAsJsonAsync("/api/usuarios", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<CreateUserResponse>();
        Assert.NotNull(created);
        Assert.False(string.IsNullOrWhiteSpace(created!.UserId));

        return (created, response.Headers.Location?.ToString());
    }

    private static async Task<string?> ReadProblemDetailsExtensionAsync(HttpResponseMessage response, string key)
    {
        var json = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(json))
            return null;

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (!root.TryGetProperty("extensions", out var ext))
            return null;

        if (!ext.TryGetProperty(key, out var value))
            return null;

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    // ---------------------------
    // AUTHZ (401/403)
    // Controller-level esperado: [Authorize(Roles = "Master,Admin")]
    // ---------------------------

    [Fact(DisplayName = "[API-INT][UsuariosController][POST] /api/usuarios -> 401 Unauthorized | deve exigir autenticação")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "UsuariosController")]
    [Trait("Method", "POST")]
    [Trait("Endpoint", "/api/usuarios")]
    public async Task Post_Usuarios_DeveRetornar401_QuandoNaoAutenticado()
    {
        var client = CreateClient();

        var req = CreateValidRequest(
            email: UniqueEmail("unauth"),
            userName: "unauth",
            roles: new[] { "Admin" });

        var response = await client.PostAsJsonAsync("/api/usuarios", req);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "[API-INT][UsuariosController][GET] /api/usuarios/{id} -> 401 Unauthorized | deve exigir autenticação")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "UsuariosController")]
    [Trait("Method", "GET")]
    [Trait("Endpoint", "/api/usuarios/{id}")]
    public async Task Get_UsuariosById_DeveRetornar401_QuandoNaoAutenticado()
    {
        var client = CreateClient();

        var response = await client.GetAsync($"/api/usuarios/{Guid.NewGuid():N}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "[API-INT][UsuariosController][GET] /api/usuarios/{id} -> 403 Forbidden | role não permitida")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "UsuariosController")]
    [Trait("Method", "GET")]
    [Trait("Endpoint", "/api/usuarios/{id}")]
    public async Task Get_UsuariosById_DeveRetornar403_QuandoRoleNaoPermitida()
    {
        var client = CreateClient();

        // Arrange: cria um usuário com role Tecnico via Master
        var masterToken = await LoginAsPrivilegedAsync(client);
        SetBearer(client, masterToken);

        var tecnicoEmail = UniqueEmail("tecnico");
        var tecnicoPwd = "Tecnico@1234";

        _ = await CreateUserAsync(client, CreateValidRequest(tecnicoEmail, "tecnico-user", tecnicoPwd, roles: new[] { "Tecnico" }));

        // login como técnico
        var tecnicoToken = await LoginAsync(client, tecnicoEmail, tecnicoPwd);

        // Act: tentar acessar endpoint protegido por Master/Admin
        client.DefaultRequestHeaders.Authorization = null;
        SetBearer(client, tecnicoToken);

        var response = await client.GetAsync($"/api/usuarios/{Guid.NewGuid():N}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact(DisplayName = "[API-INT][UsuariosController][POST] /api/usuarios -> 403 Forbidden | role não permitida")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "UsuariosController")]
    [Trait("Method", "POST")]
    [Trait("Endpoint", "/api/usuarios")]
    public async Task Post_Usuarios_DeveRetornar403_QuandoRoleNaoPermitida()
    {
        var client = CreateClient();

        // Arrange: cria um usuário com role Atendente via Master
        var masterToken = await LoginAsPrivilegedAsync(client);
        SetBearer(client, masterToken);

        var atendenteEmail = UniqueEmail("atendente");
        var atendentePwd = "Atendente@1234";

        _ = await CreateUserAsync(client, CreateValidRequest(atendenteEmail, "atendente-user", atendentePwd, roles: new[] { "Atendente" }));

        // login como atendente
        var atendenteToken = await LoginAsync(client, atendenteEmail, atendentePwd);

        client.DefaultRequestHeaders.Authorization = null;
        SetBearer(client, atendenteToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/usuarios",
            CreateValidRequest(UniqueEmail("blocked"), "blocked", "User@1234", roles: new[] { "Tecnico" }));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ---------------------------
    // POST /api/usuarios
    // ---------------------------

    [Fact(DisplayName = "[API-INT][UsuariosController][POST] /api/usuarios -> 201 Created | Master pode criar usuário")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "UsuariosController")]
    [Trait("Method", "POST")]
    [Trait("Endpoint", "/api/usuarios")]
    public async Task Post_Usuarios_DeveRetornar201_QuandoMaster()
    {
        var client = CreateClient();

        var token = await LoginAsPrivilegedAsync(client);
        SetBearer(client, token);

        var email = UniqueEmail("master.create");

        var (created, location) = await CreateUserAsync(client, CreateValidRequest(
            email: email,
            userName: "usuario-master",
            password: "User@1234",
            roles: new[] { "Admin" },
            claims: new[]
            {
                new ClaimDto { Type = "empresa_id", Value = "1" },
                new ClaimDto { Type = "unidade_id", Value = "1" }
            }));

        Assert.Equal(email, created.Email);

        // validar CreatedAtAction funcionando
        var getUrl = !string.IsNullOrWhiteSpace(location) ? location : $"/api/usuarios/{created.UserId}";
        var getResp = await client.GetAsync(getUrl);

        Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);

        var details = await getResp.Content.ReadFromJsonAsync<UserDetailsResponse>();
        Assert.NotNull(details);
        Assert.Equal(created.UserId, details!.UserId);
        Assert.Contains("Admin", details.Roles);
        Assert.Contains(details.Claims, c => c.Type == "empresa_id" && c.Value == "1");
        Assert.Contains(details.Claims, c => c.Type == "unidade_id" && c.Value == "1");
    }

    [Fact(DisplayName = "[API-INT][UsuariosController][POST] /api/usuarios -> 201 Created | Admin pode criar usuário")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "UsuariosController")]
    [Trait("Method", "POST")]
    [Trait("Endpoint", "/api/usuarios")]
    public async Task Post_Usuarios_DeveRetornar201_QuandoAdmin()
    {
        var client = CreateClient();

        // Arrange: cria um Admin via Master
        var masterToken = await LoginAsPrivilegedAsync(client);
        SetBearer(client, masterToken);

        var adminEmail = UniqueEmail("admin");
        var adminPwd = "Admin@1234";

        _ = await CreateUserAsync(client, CreateValidRequest(adminEmail, "admin-user", adminPwd, roles: new[] { "Admin" }));

        // login como Admin
        var adminToken = await LoginAsync(client, adminEmail, adminPwd);

        client.DefaultRequestHeaders.Authorization = null;
        SetBearer(client, adminToken);

        // Act: Admin criando outro usuário
        var email = UniqueEmail("admin.create");
        var response = await client.PostAsJsonAsync("/api/usuarios", CreateValidRequest(email, "usuario-admin", "User@1234", roles: new[] { "Tecnico" }));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact(DisplayName = "[API-INT][UsuariosController][POST] /api/usuarios -> 409 Conflict | e-mail já existe")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "UsuariosController")]
    [Trait("Method", "POST")]
    [Trait("Endpoint", "/api/usuarios")]
    public async Task Post_Usuarios_DeveRetornar409_QuandoEmailJaExiste()
    {
        var client = CreateClient();

        var token = await LoginAsPrivilegedAsync(client);
        SetBearer(client, token);

        var email = UniqueEmail("conflict");
        var req = CreateValidRequest(email, "conflict-user", "User@1234");

        // primeira criação
        var first = await client.PostAsJsonAsync("/api/usuarios", req);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        // segunda tentativa
        var second = await client.PostAsJsonAsync("/api/usuarios", req);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);

        var errorCode = await ReadProblemDetailsExtensionAsync(second, "errorCode");
        Assert.Equal("Conflict", errorCode);
    }

    [Fact(DisplayName = "[API-INT][UsuariosController][POST] /api/usuarios -> 400 BadRequest | body null (Validation)")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "UsuariosController")]
    [Trait("Method", "POST")]
    [Trait("Endpoint", "/api/usuarios")]
    public async Task Post_Usuarios_DeveRetornar400_QuandoBodyNull()
    {
        var client = CreateClient();

        var token = await LoginAsPrivilegedAsync(client);
        SetBearer(client, token);

        // envia JSON literal null para forçar request == null no binding
        var message = new HttpRequestMessage(HttpMethod.Post, "/api/usuarios")
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        var response = await client.SendAsync(message);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errorCode = await ReadProblemDetailsExtensionAsync(response, "errorCode");
        Assert.Equal("Validation", errorCode);
    }

    [Fact(DisplayName = "[API-INT][UsuariosController][POST] /api/usuarios -> 400 BadRequest | validação de request (Validation)")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "UsuariosController")]
    [Trait("Method", "POST")]
    [Trait("Endpoint", "/api/usuarios")]
    public async Task Post_Usuarios_DeveRetornar400_QuandoValidacaoFalha()
    {
        var client = CreateClient();

        var token = await LoginAsPrivilegedAsync(client);
        SetBearer(client, token);

        var invalid = new CreateUserRequest
        {
            UserName = "",
            Email = "",
            Password = "",
            ConfirmPassword = ""
        };

        var response = await client.PostAsJsonAsync("/api/usuarios", invalid);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errorCode = await ReadProblemDetailsExtensionAsync(response, "errorCode");
        Assert.Equal("Validation", errorCode);
    }

    [Fact(DisplayName = "[API-INT][UsuariosController][POST] /api/usuarios -> 400 BadRequest | IdentityError (senha fraca)")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "UsuariosController")]
    [Trait("Method", "POST")]
    [Trait("Endpoint", "/api/usuarios")]
    public async Task Post_Usuarios_DeveRetornar400_QuandoIdentityError()
    {
        var client = CreateClient();

        var token = await LoginAsPrivilegedAsync(client);
        SetBearer(client, token);

        // password viola a policy (sem maiúscula / caractere especial, etc.)
        var req = CreateValidRequest(
            email: UniqueEmail("weak"),
            userName: "weak-user",
            password: "password1",
            roles: new[] { "Tecnico" });

        var response = await client.PostAsJsonAsync("/api/usuarios", req);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errorCode = await ReadProblemDetailsExtensionAsync(response, "errorCode");
        Assert.Equal("IdentityError", errorCode);
    }

    // ---------------------------
    // GET /api/usuarios/{id}
    // ---------------------------

    [Fact(DisplayName = "[API-INT][UsuariosController][GET] /api/usuarios/{id} -> 200 OK | usuário existe")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "UsuariosController")]
    [Trait("Method", "GET")]
    [Trait("Endpoint", "/api/usuarios/{id}")]
    public async Task Get_UsuariosById_DeveRetornar200_QuandoExiste()
    {
        var client = CreateClient();

        var token = await LoginAsPrivilegedAsync(client);
        SetBearer(client, token);

        var email = UniqueEmail("get");
        var (created, _) = await CreateUserAsync(client, CreateValidRequest(email, "user-get", "User@1234", roles: new[] { "Admin" }));

        var response = await client.GetAsync($"/api/usuarios/{created.UserId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var details = await response.Content.ReadFromJsonAsync<UserDetailsResponse>();
        Assert.NotNull(details);
        Assert.Equal(created.UserId, details!.UserId);
        Assert.Equal(email, details.Email);
    }

    [Fact(DisplayName = "[API-INT][UsuariosController][GET] /api/usuarios/{id} -> 404 NotFound | usuário não existe")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "UsuariosController")]
    [Trait("Method", "GET")]
    [Trait("Endpoint", "/api/usuarios/{id}")]
    public async Task Get_UsuariosById_DeveRetornar404_QuandoNaoExiste()
    {
        var client = CreateClient();

        var token = await LoginAsPrivilegedAsync(client);
        SetBearer(client, token);

        var response = await client.GetAsync($"/api/usuarios/{Guid.NewGuid():N}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "[API-INT][UsuariosController][GET] /api/usuarios/{id} -> 400 BadRequest | id inválido")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "UsuariosController")]
    [Trait("Method", "GET")]
    [Trait("Endpoint", "/api/usuarios/{id}")]
    public async Task Get_UsuariosById_DeveRetornar400_QuandoIdInvalido()
    {
        var client = CreateClient();

        var token = await LoginAsPrivilegedAsync(client);
        SetBearer(client, token);

        // id = espaço (url encoded)
        var response = await client.GetAsync("/api/usuarios/%20");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // neste endpoint, não há errorCode em Extensions — somente errors.
        var json = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(json));
    }
}
