using Microsoft.Extensions.DependencyInjection;
using OsLog.Application.Common.Responses;
using OsLog.Application.DTOs.Empresa;
using OsLog.Application.DTOs.Unidade;
using OsLog.Infrastructure.EntityFramework;
using OsLog.Tests.Shared.Factories;
using System.Net;
using System.Net.Http.Json;

namespace OsLog.Application.Tests.Integration.Api.Controllers;

public sealed class EmpresaControllerIntegrationTests : IClassFixture<OsLogApiFactory>
{
    private readonly OsLogApiFactory _factory;
    private readonly HttpClient _client;

    public EmpresaControllerIntegrationTests(OsLogApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // ---------------------------------------------------------------------
    // Test helpers
    // ---------------------------------------------------------------------

    private async Task ResetDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    private static EmpresaCreateDto NewEmpresaDto(
        string? razaoSocial = null,
        string? nomeFantasia = null,
        string? cnpj = null)
        => new()
        {
            RazaoSocial = razaoSocial ?? $"Empresa {Guid.NewGuid():N}",
            NomeFantasia = nomeFantasia ?? "Empresa Teste",
            Cnpj = cnpj ?? OsLog.Tests.Shared.Generators.CnpjGenerator.GenerateValidCnpj(seed: 231)
        };

    private async Task<int> CriarEmpresaAsync(EmpresaCreateDto? dto = null)
    {
        dto ??= NewEmpresaDto();

        var response = await _client.PostAsJsonAsync("/api/empresas", dto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var envelope = await response.Content.ReadFromJsonAsync<OsLogResponse<IdPayload>>();
        Assert.NotNull(envelope);
        Assert.True(envelope!.Sucesso);
        Assert.NotNull(envelope.Dados);
        Assert.True(envelope.Dados!.Id > 0);

        return envelope.Dados.Id;
    }

    private sealed record IdPayload(int Id);

    private sealed record IdEmpresaPayload(int Id, int EmpresaId);

    private static void AssertEnvelopeCriticaValidacao(OsLogResponse<object>? envelope)
    {
        Assert.NotNull(envelope);
        Assert.False(envelope!.Sucesso);
        Assert.Equal(CodigosOsLog.ERRO_VALIDACAO, envelope.Codigo);
        Assert.NotNull(envelope.Erros);
    }

    // ---------------------------------------------------------------------
    // POST /api/empresas
    // ---------------------------------------------------------------------

    [Fact(DisplayName = "[API-INT][EmpresaController][POST] /api/empresas -> 201 Created | Deve criar empresa e criar Unidade Matriz")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "EmpresaController")]
    [Trait("HTTP", "POST")]
    public async Task Post_Empresas_DeveRetornar201_ComEmpresaCriada_E_UnidadeMatriz()
    {
        await ResetDatabaseAsync();

        var dto = NewEmpresaDto(razaoSocial: "Empresa Integração", nomeFantasia: "Emp Int");

        var response = await _client.PostAsJsonAsync("/api/empresas", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var envelope = await response.Content.ReadFromJsonAsync<OsLogResponse<IdPayload>>();
        Assert.NotNull(envelope);
        Assert.True(envelope!.Sucesso);
        Assert.NotNull(envelope.Dados);
        Assert.True(envelope.Dados!.Id > 0);

        // A camada de serviço cria automaticamente a unidade "Matriz".
        var empresaId = envelope.Dados.Id;
        var unidadesResponse = await _client.GetAsync($"/api/empresas/{empresaId}/unidades");
        Assert.Equal(HttpStatusCode.OK, unidadesResponse.StatusCode);

        var unidadesEnvelope = await unidadesResponse.Content
            .ReadFromJsonAsync<OsLogResponse<IEnumerable<UnidadeDto>>>();

        Assert.NotNull(unidadesEnvelope);
        Assert.True(unidadesEnvelope!.Sucesso);
        Assert.NotNull(unidadesEnvelope.Dados);
        Assert.Contains(unidadesEnvelope.Dados!, u => u.EmpresaId == empresaId && u.Nome == "Matriz");
    }

    [Fact(DisplayName = "[API-INT][EmpresaController][POST] /api/empresas -> 400 BadRequest | Deve retornar ERRO_VALIDACAO quando DTO inválido")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "EmpresaController")]
    [Trait("HTTP", "POST")]
    public async Task Post_Empresas_DeveRetornar400_ComErroValidacao_QuandoDtoInvalido()
    {
        await ResetDatabaseAsync();

        var dto = new EmpresaCreateDto
        {
            RazaoSocial = "",
            NomeFantasia = "",
            Cnpj = "123" // falha no Regex ^\d{14}$
        };

        var response = await _client.PostAsJsonAsync("/api/empresas", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var envelope = await response.Content.ReadFromJsonAsync<OsLogResponse<object>>();
        AssertEnvelopeCriticaValidacao(envelope);
    }

    // ---------------------------------------------------------------------
    // GET /api/empresas
    // ---------------------------------------------------------------------

    [Fact(DisplayName = "[API-INT][EmpresaController][GET] /api/empresas -> 200 OK | Deve retornar lista vazia quando não houver empresas")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "EmpresaController")]
    [Trait("HTTP", "GET")]
    public async Task Get_Empresas_DeveRetornar200_ComListaVazia_QuandoNaoHouverEmpresas()
    {
        await ResetDatabaseAsync();

        var response = await _client.GetAsync("/api/empresas");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var envelope = await response.Content.ReadFromJsonAsync<OsLogResponse<IEnumerable<EmpresaListDto>>>();
        Assert.NotNull(envelope);
        Assert.True(envelope!.Sucesso);
        Assert.NotNull(envelope.Dados);
        Assert.Empty(envelope.Dados!);
    }

    [Fact(DisplayName = "[API-INT][EmpresaController][GET] /api/empresas -> 200 OK | Deve retornar empresa criada")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "EmpresaController")]
    [Trait("HTTP", "GET")]
    public async Task Get_Empresas_DeveRetornar200_ComEmpresas_QuandoExistirem()
    {
        await ResetDatabaseAsync();

        var id = await CriarEmpresaAsync(NewEmpresaDto(razaoSocial: "Empresa Lista", nomeFantasia: "Emp Lista"));

        var response = await _client.GetAsync("/api/empresas");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var envelope = await response.Content.ReadFromJsonAsync<OsLogResponse<IEnumerable<EmpresaListDto>>>();
        Assert.NotNull(envelope);
        Assert.True(envelope!.Sucesso);
        Assert.NotNull(envelope.Dados);
        Assert.Contains(envelope.Dados!, e => e.Id == id);
    }

    // ---------------------------------------------------------------------
    // GET /api/empresas/{id}
    // ---------------------------------------------------------------------

    [Fact(DisplayName = "[API-INT][EmpresaController][GET] /api/empresas/{id} -> 200 OK | Deve retornar detalhes quando existir")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "EmpresaController")]
    [Trait("HTTP", "GET")]
    public async Task Get_EmpresasPorId_DeveRetornar200_QuandoEmpresaExiste()
    {
        await ResetDatabaseAsync();

        var id = await CriarEmpresaAsync(NewEmpresaDto(razaoSocial: "Empresa GetById", nomeFantasia: "Emp GetById"));

        var response = await _client.GetAsync($"/api/empresas/{id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var envelope = await response.Content.ReadFromJsonAsync<OsLogResponse<EmpresaDetailDto>>();
        Assert.NotNull(envelope);
        Assert.True(envelope!.Sucesso);
        Assert.NotNull(envelope.Dados);
        Assert.Equal(id, envelope.Dados!.Id);
    }

    [Fact(DisplayName = "[API-INT][EmpresaController][GET] /api/empresas/{id} -> 404 NotFound | Deve retornar EMPRESA_NAO_ENCONTRADA quando não existir")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "EmpresaController")]
    [Trait("HTTP", "GET")]
    public async Task Get_EmpresasPorId_DeveRetornar404_QuandoNaoExiste()
    {
        await ResetDatabaseAsync();

        const int idInexistente = 999999;

        var response = await _client.GetAsync($"/api/empresas/{idInexistente}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var envelope = await response.Content.ReadFromJsonAsync<OsLogResponse<EmpresaDetailDto>>();
        Assert.NotNull(envelope);
        Assert.False(envelope!.Sucesso);
        Assert.Equal(CodigosOsLog.EMPRESA_NAO_ENCONTRADA, envelope.Codigo);
        Assert.Null(envelope.Dados);
    }

    // ---------------------------------------------------------------------
    // DELETE /api/empresas/{id}
    // ---------------------------------------------------------------------

    [Fact(DisplayName = "[API-INT][EmpresaController][DELETE] /api/empresas/{id} -> 204 NoContent | Deve excluir com sucesso")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "EmpresaController")]
    [Trait("HTTP", "DELETE")]
    public async Task Delete_Empresas_DeveRetornar204_QuandoSucesso()
    {
        await ResetDatabaseAsync();

        var id = await CriarEmpresaAsync(NewEmpresaDto(razaoSocial: "Empresa Delete", nomeFantasia: "Emp Delete"));

        var response = await _client.DeleteAsync($"/api/empresas/{id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Confirma que ficou inacessível via GET
        var getResponse = await _client.GetAsync($"/api/empresas/{id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact(DisplayName = "[API-INT][EmpresaController][DELETE] /api/empresas/{id} -> 404 NotFound | Deve retornar EMPRESA_NAO_ENCONTRADA quando não existir")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "EmpresaController")]
    [Trait("HTTP", "DELETE")]
    public async Task Delete_Empresas_DeveRetornar404_QuandoNaoExiste()
    {
        await ResetDatabaseAsync();

        const int idInexistente = 999999;

        var response = await _client.DeleteAsync($"/api/empresas/{idInexistente}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var envelope = await response.Content.ReadFromJsonAsync<OsLogResponse<object>>();
        Assert.NotNull(envelope);
        Assert.False(envelope!.Sucesso);
        Assert.Equal(CodigosOsLog.EMPRESA_NAO_ENCONTRADA, envelope.Codigo);
        Assert.Null(envelope.Dados);
    }

    // ---------------------------------------------------------------------
    // POST /api/empresas/{empresaId}/unidades
    // ---------------------------------------------------------------------

    [Fact(DisplayName = "[API-INT][EmpresaController][POST] /api/empresas/{empresaId}/unidades -> 201 Created | Deve criar unidade com sucesso")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "EmpresaController")]
    [Trait("HTTP", "POST")]
    public async Task Post_UnidadesPorEmpresa_DeveRetornar201_QuandoDtoValido()
    {
        await ResetDatabaseAsync();

        var empresaId = await CriarEmpresaAsync(NewEmpresaDto(razaoSocial: "Empresa Unidade", nomeFantasia: "Emp Unidade"));
        var dto = new UnidadeCreateDto { Nome = "Filial 1", Cnpj = OsLog.Tests.Shared.Generators.CnpjGenerator.GenerateValidCnpj() };

        var response = await _client.PostAsJsonAsync($"/api/empresas/{empresaId}/unidades", dto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var envelope = await response.Content.ReadFromJsonAsync<OsLogResponse<IdEmpresaPayload>>();
        Assert.NotNull(envelope);
        Assert.True(envelope!.Sucesso);
        Assert.NotNull(envelope.Dados);
        Assert.True(envelope.Dados!.Id > 0);
        Assert.Equal(empresaId, envelope.Dados.EmpresaId);

        // Agora deve haver pelo menos Matriz + Filial 1
        var listResponse = await _client.GetAsync($"/api/empresas/{empresaId}/unidades");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var listEnvelope = await listResponse.Content.ReadFromJsonAsync<OsLogResponse<IEnumerable<UnidadeDto>>>();
        Assert.NotNull(listEnvelope);
        Assert.True(listEnvelope!.Sucesso);
        Assert.NotNull(listEnvelope.Dados);
        Assert.Contains(listEnvelope.Dados!, u => u.Nome == "Matriz");
        Assert.Contains(listEnvelope.Dados!, u => u.Nome == "Filial 1");
    }

    [Fact(DisplayName = "[API-INT][EmpresaController][POST] /api/empresas/{empresaId}/unidades -> 400 BadRequest | Deve retornar ERRO_VALIDACAO quando DTO inválido")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "EmpresaController")]
    [Trait("HTTP", "POST")]
    public async Task Post_UnidadesPorEmpresa_DeveRetornar400_ComErroValidacao_QuandoDtoInvalido()
    {
        await ResetDatabaseAsync();

        var empresaId = await CriarEmpresaAsync(NewEmpresaDto(razaoSocial: "Empresa Unidade Inválida", nomeFantasia: "Emp Unid Inv"));
        var dto = new UnidadeCreateDto { Nome = "", Cnpj = "123" };

        var response = await _client.PostAsJsonAsync($"/api/empresas/{empresaId}/unidades", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var envelope = await response.Content.ReadFromJsonAsync<OsLogResponse<object>>();
        AssertEnvelopeCriticaValidacao(envelope);
    }

    // ---------------------------------------------------------------------
    // GET /api/empresas/{empresaId}/unidades
    // ---------------------------------------------------------------------

    [Fact(DisplayName = "[API-INT][EmpresaController][GET] /api/empresas/{empresaId}/unidades -> 200 OK | Deve retornar unidades (inclui Matriz)")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "EmpresaController")]
    [Trait("HTTP", "GET")]
    public async Task Get_UnidadesPorEmpresa_DeveRetornar200_ComUnidadesDaEmpresa_IncluindoMatriz()
    {
        await ResetDatabaseAsync();

        var empresaId = await CriarEmpresaAsync(NewEmpresaDto(razaoSocial: "Empresa Sem Unidades Extras", nomeFantasia: "Emp Sem Unidades"));

        var response = await _client.GetAsync($"/api/empresas/{empresaId}/unidades");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var envelope = await response.Content.ReadFromJsonAsync<OsLogResponse<IEnumerable<UnidadeDto>>>();
        Assert.NotNull(envelope);
        Assert.True(envelope!.Sucesso);
        Assert.NotNull(envelope.Dados);

        var unidades = envelope.Dados!.ToList();
        Assert.NotEmpty(unidades);
        Assert.All(unidades, u => Assert.Equal(empresaId, u.EmpresaId));
        Assert.Contains(unidades, u => u.Nome == "Matriz");
    }

    // ---------------------------------------------------------------------
    // GET /api/unidades
    // ---------------------------------------------------------------------

    [Fact(DisplayName = "[API-INT][EmpresaController][GET] /api/unidades -> 200 OK | Deve listar unidades de todas as empresas")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "EmpresaController")]
    [Trait("HTTP", "GET")]
    public async Task Get_Unidades_DeveRetornar200_ComUnidadesDeTodasEmpresas()
    {
        await ResetDatabaseAsync();

        var empresaA = await CriarEmpresaAsync(NewEmpresaDto(razaoSocial: "Empresa A", nomeFantasia: "Emp A"));
        var empresaB = await CriarEmpresaAsync(NewEmpresaDto(razaoSocial: "Empresa B", nomeFantasia: "Emp B"));

        // Cada empresa cria automaticamente uma unidade "Matriz"
        var response = await _client.GetAsync("/api/unidades");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var envelope = await response.Content.ReadFromJsonAsync<OsLogResponse<IEnumerable<UnidadeDto>>>();
        Assert.NotNull(envelope);
        Assert.True(envelope!.Sucesso);
        Assert.NotNull(envelope.Dados);

        var unidades = envelope.Dados!.ToList();
        Assert.True(unidades.Count >= 2);
        Assert.Contains(unidades, u => u.EmpresaId == empresaA);
        Assert.Contains(unidades, u => u.EmpresaId == empresaB);
    }
}
