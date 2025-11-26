using System.Net;
using System.Net.Http.Json;
using OsLog.Application.Common.Responses;
using OsLog.Application.DTOs.Empresa;
using OsLog.Application.Common; // CodigosOsLog / CriticasOsLog
using OsLog.Tests.Integration.Helpers;
using Xunit;

namespace OsLog.Tests.Integration.Api;

public class EmpresaControllerIntegrationTests : IClassFixture<OsLogApiFactory>
{
    private readonly HttpClient _client;

    public EmpresaControllerIntegrationTests(OsLogApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ---------------------------
    // Helpers
    // ---------------------------

    private async Task<int> CriarEmpresaAsync(
        string razaoSocial = "ConsertaSmart ME",
        string nomeFantasia = "ConsertaSmart Centro",
        string cnpj = "12345678000199") // 14 dígitos (DataAnnotation!)
    {
        var dto = new EmpresaCreateDto
        {
            RazaoSocial = razaoSocial,
            NomeFantasia = nomeFantasia,
            Cnpj = cnpj
        };

        var response = await _client.PostAsJsonAsync("/api/empresas", dto);
        response.EnsureSuccessStatusCode(); // 201

        var envelope =
            await response.Content.ReadFromJsonAsync<OsLogResponse<IdPayload>>();

        Assert.NotNull(envelope);
        Assert.True(envelope!.Sucesso);
        Assert.NotNull(envelope.Dados);

        return envelope.Dados!.Id;
    }

    // ---------------------------
    // CREATE EMPRESA
    // ---------------------------

    [Fact(DisplayName = "[INT] POST /api/empresas - deve criar empresa com sucesso")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task Create_Deve_Criar_Empresa_Com_Sucesso()
    {
        // Arrange
        var dto = new EmpresaCreateDto
        {
            RazaoSocial = "Empresa Integração",
            NomeFantasia = "Emp Int",
            Cnpj = "99887766000111"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/empresas", dto);

        // Assert HTTP
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var envelope =
            await response.Content.ReadFromJsonAsync<OsLogResponse<IdPayload>>();

        Assert.NotNull(envelope);
        Assert.True(envelope!.Sucesso);
        Assert.Equal("Empresa criada com sucesso.", envelope.Mensagem);
        Assert.NotNull(envelope.Dados);
        Assert.True(envelope.Dados!.Id > 0);
    }

    [Fact(DisplayName = "[API-INT] POST /api/empresas deve retornar ERRO_VALIDACAO quando ModelState inválido")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task Create_Deve_Retornar_ErroValidacao_Quando_ModelState_Invalido()
    {
        // Arrange: RazaoSocial vazia, NomeFantasia vazio, CNPJ inválido
        var dto = new EmpresaCreateDto
        {
            RazaoSocial = "",
            NomeFantasia = "",
            Cnpj = "123" // não passa na regex
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/empresas", dto);

        // Assert HTTP
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var envelope = await response.Content
            .ReadFromJsonAsync<OsLogResponse<object>>();

        Assert.NotNull(envelope);

        // Sucesso false + código de negócio de validação
        Assert.False(envelope!.Sucesso);
        Assert.Equal(CodigosOsLog.ERRO_VALIDACAO, envelope.Codigo);

        // Mensagem de erro não pode ser vazia
        Assert.False(string.IsNullOrWhiteSpace(envelope.Mensagem));

        // Pelo seu contrato atual:
        // - Dados deve ser null em erro
        // - Erros contém o ModelState serializado (não vamos inspecionar o conteúdo)
        Assert.Null(envelope.Dados);
        Assert.NotNull(envelope.Erros);
    }


    //[Fact(DisplayName = "[INT] POST /api/empresas - deve retornar erro de validação quando ModelState inválido")]
    //[Trait("Category", "Integration")]
    //[Trait("SubCategory", "EmpresaController")]
    //public async Task Create_Deve_Retornar_ErroValidacao_Quando_ModelState_Invalido()
    //{
    //    // Arrange: RazaoSocial vazia, CNPJ inválido
    //    var dto = new EmpresaCreateDto
    //    {
    //        RazaoSocial = "",
    //        NomeFantasia = "",
    //        Cnpj = "123" // não passa na regex
    //    };

    //    // Act
    //    var response = await _client.PostAsJsonAsync("/api/empresas", dto);

    //    // Assert HTTP
    //    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

    //    var envelope =
    //        await response.Content.ReadFromJsonAsync<OsLogResponse<object>>();

    //    Assert.NotNull(envelope);
    //    Assert.False(envelope!.Sucesso);
    //    Assert.Equal(CodigosOsLog.ERRO_VALIDACAO, envelope.Codigo);

    //    // Não vamos desserializar ModelState, apenas garantir que há mensagem
    //    Assert.False(string.IsNullOrWhiteSpace(envelope.Mensagem));
    //    Assert.NotNull(envelope.Dados); // aqui é o ModelState serializado
    //}

    // ---------------------------
    // GET ALL
    // ---------------------------

    [Fact(DisplayName = "[INT] GET /api/empresas - deve retornar lista dentro de OsLogResponse.Ok")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task GetAll_Deve_Retornar_Lista()
    {
        // Arrange: garante que existe pelo menos 1
        _ = await CriarEmpresaAsync(
            "Empresa Lista",
            "Emp Lista",
            "11111111000111");

        // Act
        var response = await _client.GetAsync("/api/empresas");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var envelope =
            await response.Content.ReadFromJsonAsync<OsLogResponse<IEnumerable<EmpresaListDto>>>();

        Assert.NotNull(envelope);
        Assert.True(envelope!.Sucesso);
        Assert.NotNull(envelope.Dados);
        Assert.NotEmpty(envelope.Dados!);
    }

    // ---------------------------
    // GET BY ID
    // ---------------------------

    [Fact(DisplayName = "[INT] GET /api/empresas/{id} - deve retornar empresa quando existir")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task GetById_Deve_Retornar_Empresa_Quando_Existe()
    {
        // Arrange
        var id = await CriarEmpresaAsync(
            "Empresa GetById",
            "Emp GetById",
            "22222222000111");

        // Act
        var response = await _client.GetAsync($"/api/empresas/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var envelope =
            await response.Content.ReadFromJsonAsync<OsLogResponse<EmpresaDetailDto>>();

        Assert.NotNull(envelope);
        Assert.True(envelope!.Sucesso);
        Assert.NotNull(envelope.Dados);
        Assert.Equal(id, envelope.Dados!.Id);
    }

    [Fact(DisplayName = "[INT] GET /api/empresas/{id} - deve retornar NotFound quando não existir")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task GetById_Deve_Retornar_NotFound_Quando_Nao_Existe()
    {
        // Arrange: um id alto improvável
        const int idInexistente = 999999;

        // Act
        var response = await _client.GetAsync($"/api/empresas/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var envelope =
            await response.Content.ReadFromJsonAsync<OsLogResponse<EmpresaDetailDto>>();

        Assert.NotNull(envelope);
        Assert.False(envelope!.Sucesso);
        Assert.Equal(CodigosOsLog.EMPRESA_NAO_ENCONTRADA, envelope.Codigo);
        Assert.Null(envelope.Dados);
    }

    // ---------------------------
    // DELETE
    // ---------------------------

    [Fact(DisplayName = "[INT] DELETE /api/empresas/{id} - deve retornar NoContent quando excluir com sucesso")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task Delete_Deve_Retornar_NoContent_Quando_Sucesso()
    {
        // Arrange
        var id = await CriarEmpresaAsync(
            "Empresa Delete",
            "Emp Delete",
            "33333333000111");

        // Act
        var response = await _client.DeleteAsync($"/api/empresas/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        // Sem body (controller retorna NoContent())
    }

    [Fact(DisplayName = "[INT] DELETE /api/empresas/{id} - deve retornar NotFound quando empresa não existir")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task Delete_Deve_Retornar_NotFound_Quando_Nao_Existe()
    {
        // Arrange
        const int idInexistente = 999999;

        // Act
        var response = await _client.DeleteAsync($"/api/empresas/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var envelope =
            await response.Content.ReadFromJsonAsync<OsLogResponse<object>>();

        Assert.NotNull(envelope);
        Assert.False(envelope!.Sucesso);
        Assert.Equal(CodigosOsLog.EMPRESA_NAO_ENCONTRADA, envelope.Codigo);
        Assert.Null(envelope.Dados);
    }

    // ---------------------------
    // UNIDADES
    // ---------------------------

    [Fact(DisplayName = "[INT] POST /api/empresas/{empresaId}/unidades - deve criar unidade com sucesso")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task CriarUnidade_Deve_Criar_Unidade_Com_Sucesso()
    {
        // Arrange: empresa base
        var empresaId = await CriarEmpresaAsync(
            "Empresa Unidade",
            "Emp Unidade",
            "44444444000111");

        var dto = new UnidadeCreateDto
        {
            Nome = "Matriz",
            Cnpj = "55555555000111" // se tiver validação, use 14 dígitos
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/empresas/{empresaId}/unidades", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var envelope =
            await response.Content.ReadFromJsonAsync<OsLogResponse<IdEmpresaPayload>>();

        Assert.NotNull(envelope);
        Assert.True(envelope!.Sucesso);
        Assert.NotNull(envelope.Dados);
        Assert.True(envelope.Dados!.Id > 0);
        Assert.Equal(empresaId, envelope.Dados!.EmpresaId);
    }

    // payload específico para unidade (Id + EmpresaId)
    private sealed record IdEmpresaPayload(int Id, int EmpresaId);

    [Fact(DisplayName = "[INT] POST /api/empresas/{empresaId}/unidades - deve retornar erro de validação quando ModelState inválido")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task CriarUnidade_Deve_Retornar_ErroValidacao_Quando_ModelState_Invalido()
    {
        // Arrange
        var empresaId = await CriarEmpresaAsync(
            "Empresa Unidade Inválida",
            "Emp Unid Inv",
            "66666666000111");

        var dto = new UnidadeCreateDto
        {
            Nome = "",   // inválido
            Cnpj = "123" // se tiver regex
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/empresas/{empresaId}/unidades", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var envelope =
            await response.Content.ReadFromJsonAsync<OsLogResponse<object>>();

        Assert.NotNull(envelope);
        Assert.False(envelope!.Sucesso);
        Assert.Equal(CodigosOsLog.ERRO_VALIDACAO, envelope.Codigo);
        Assert.Null(envelope.Dados);
    }

    [Fact(DisplayName = "[INT] GET /api/empresas/{empresaId}/unidades - deve listar unidades da empresa")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task ListarUnidades_Deve_Retornar_Unidades_Da_Empresa()
    {
        // Arrange
        var empresaId = await CriarEmpresaAsync(
            "Empresa Listar Unidades",
            "Emp Listar Unid",
            "77777777000111");

        var dto = new UnidadeCreateDto
        {
            Nome = "Filial 1",
            Cnpj = "88888888000111"
        };

        // cria uma unidade
        var createResponse =
            await _client.PostAsJsonAsync($"/api/empresas/{empresaId}/unidades", dto);

        createResponse.EnsureSuccessStatusCode();

        // Act
        var response =
            await _client.GetAsync($"/api/empresas/{empresaId}/unidades");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var envelope =
            await response.Content.ReadFromJsonAsync<OsLogResponse<IEnumerable<UnidadeDto>>>();

        Assert.NotNull(envelope);
        Assert.True(envelope!.Sucesso);
        Assert.NotNull(envelope.Dados);

        var unidades = envelope.Dados!.ToList();
        Assert.NotEmpty(unidades);
        Assert.All(unidades, u => Assert.Equal(empresaId, u.EmpresaId));
    }

    [Fact(DisplayName = "[INT] GET /api/unidades - deve listar todas as unidades")]
    [Trait("Category", "Integration")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task ListarTodasUnidades_Deve_Retornar_Todas_As_Unidades()
    {
        // Arrange: duas empresas com uma unidade cada
        var empresa1 = await CriarEmpresaAsync(
            "Empresa A",
            "Emp A",
            "11112222000111");

        var empresa2 = await CriarEmpresaAsync(
            "Empresa B",
            "Emp B",
            "33334444000111");

        var dto1 = new UnidadeCreateDto { Nome = "Mat A", Cnpj = "99990000000111" };
        var dto2 = new UnidadeCreateDto { Nome = "Mat B", Cnpj = "99991111000111" };

        await _client.PostAsJsonAsync($"/api/empresas/{empresa1}/unidades", dto1);
        await _client.PostAsJsonAsync($"/api/empresas/{empresa2}/unidades", dto2);

        // Act
        var response = await _client.GetAsync("/api/unidades");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var envelope =
            await response.Content.ReadFromJsonAsync<OsLogResponse<IEnumerable<UnidadeDto>>>();

        Assert.NotNull(envelope);
        Assert.True(envelope!.Sucesso);
        Assert.NotNull(envelope.Dados);

        var unidades = envelope.Dados!.ToList();
        Assert.True(unidades.Count >= 2);
    }
}
