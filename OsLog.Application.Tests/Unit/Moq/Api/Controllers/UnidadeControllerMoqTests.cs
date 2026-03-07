using Microsoft.AspNetCore.Mvc;
using Moq;
using OsLog.API.Controllers;
using OsLog.Application.Common.Responses;
using OsLog.Application.DTOs.Unidade;
using OsLog.Application.Ports.ApplicationServices;

namespace OsLog.Tests.Unit.Moq.Api.Controllers;

public class UnidadeControllerMoqTests
{
    private readonly Mock<IUnidadeService> _unidadeServiceMock;
    private readonly UnidadeController _controller;

    public UnidadeControllerMoqTests()
    {
        _unidadeServiceMock = new Mock<IUnidadeService>(MockBehavior.Strict);
        _controller = new UnidadeController(_unidadeServiceMock.Object);
    }

    // =========================================================
    // POST /api/unidades/{empresaId}/unidades
    // =========================================================

    [Fact(DisplayName = "[API-MOQ] POST Unidade - deve retornar CreatedAtAction com OsLogResponse.Ok")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "UnidadeController")]
    public async Task Create_Deve_Retornar_CreatedAtAction_Com_OsLogResponseOk()
    {
        // Arrange
        var empresaId = 10;

        var dto = new UnidadeCreateDto
        {
            Nome = "ConsertaSmart Centro",
            Cnpj = "11111111000111",
            InscricaoEstadual = "123456789788",
            InscricaoMunicipal = "987654321",
            Endereco = "Rua da Consolação, 1234 - Centro, São Paulo - SP",
            Telefone = "(11) 1234-5678"
        };

        _unidadeServiceMock
            .Setup(s => s.Create(empresaId, dto, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(123);

        // Act
        var result = await _controller.Create(empresaId, dto, CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(UnidadeController.GetById), created.ActionName);
        Assert.NotNull(created.RouteValues);
        Assert.Equal(empresaId, created.RouteValues["empresaId"]);

        var response = Assert.IsType<OsLogResponse<object>>(created.Value);
        Assert.True(response.Sucesso);
        Assert.NotNull(response.Dados);
        Assert.Equal("Unidade criada com sucesso.", response.Mensagem);

        var payload = response.Dados!;
        var propId = payload.GetType().GetProperty("Id");
        var propEmpresaId = payload.GetType().GetProperty("EmpresaId");

        Assert.NotNull(propId);
        Assert.NotNull(propEmpresaId);
        Assert.Equal(123, (int)propId!.GetValue(payload)!);
        Assert.Equal(empresaId, (int)propEmpresaId!.GetValue(payload)!);

        _unidadeServiceMock.Verify(
            s => s.Create(empresaId, dto, It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "[API-MOQ] POST Unidade - deve retornar erro quando ModelState for inválido")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "UnidadeController")]
    public async Task Create_Deve_Retornar_ValidationProblem_Quando_ModelState_For_Invalido()
    {
        // Arrange
        var empresaId = 10;

        var dto = new UnidadeCreateDto
        {
            Nome = "",
            Cnpj = "11111111000111"
        };

        _controller.ModelState.AddModelError("Nome", "O campo Nome é obrigatório.");

        // Act
        var result = await _controller.Create(empresaId, dto, CancellationToken.None);

        // Assert
        Assert.IsAssignableFrom<IActionResult>(result);

        _unidadeServiceMock.Verify(
            s => s.Create(It.IsAny<int>(), It.IsAny<UnidadeCreateDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // =========================================================
    // GET /api/unidades
    // =========================================================

    [Fact(DisplayName = "[API-MOQ] GET All Unidade - deve retornar Ok com lista")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "UnidadeController")]
    public async Task GetAll_Deve_Retornar_Ok_Com_Lista()
    {
        // Arrange
        var unidades = new List<UnidadeDto>
        {
            new()
            {
                Id = 1,
                EmpresaId = 1,
                Nome = "ConsertaSmart Centro",
                Cnpj = "11111111000111",
                InscricaoEstadual = "123456789788",
                InscricaoMunicipal = "987654321",
                Endereco = "Rua A",
                Telefone = "(11) 1111-1111",
                Ativa = true
            },
            new()
            {
                Id = 2,
                EmpresaId = 1,
                Nome = "ConsertaSmart Norte",
                Cnpj = "22222222000122",
                InscricaoEstadual = "987654321000",
                InscricaoMunicipal = "123123123",
                Endereco = "Rua B",
                Telefone = "(11) 2222-2222",
                Ativa = true
            }
        };

        _unidadeServiceMock
            .Setup(s => s.GetAll(It.IsAny<CancellationToken>()))
            .ReturnsAsync(unidades);

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<OsLogResponse<IEnumerable<UnidadeDto>>>(ok.Value);

        Assert.True(response.Sucesso);
        Assert.NotNull(response.Dados);
        Assert.Equal("Unidades retornadas com sucesso.", response.Mensagem);

        var dados = response.Dados!.ToList();
        Assert.Equal(2, dados.Count);
        Assert.Equal(1, dados[0].Id);
        Assert.Equal(2, dados[1].Id);

        _unidadeServiceMock.Verify(
            s => s.GetAll(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "[API-MOQ] GET All Unidade - deve retornar NotFound quando lista estiver vazia")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "UnidadeController")]
    public async Task GetAll_Deve_Retornar_NotFound_Quando_Lista_Estiver_Vazia()
    {
        // Arrange
        _unidadeServiceMock
            .Setup(s => s.GetAll(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UnidadeDto>());

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<OsLogResponse<UnidadeCreateDto>>(notFound.Value);

        Assert.False(response.Sucesso);
        Assert.Null(response.Dados);

        _unidadeServiceMock.Verify(
            s => s.GetAll(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // =========================================================
    // GET /api/unidades/{empresaId}/unidades
    // =========================================================

    [Fact(DisplayName = "[API-MOQ] GET ById Unidade - deve retornar Ok com lista")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "UnidadeController")]
    public async Task GetById_Deve_Retornar_Ok_Com_Lista()
    {
        // Arrange
        var empresaId = 1;

        var unidades = new List<UnidadeDto>
        {
            new()
            {
                Id = 1,
                EmpresaId = empresaId,
                Nome = "ConsertaSmart Centro",
                Cnpj = "11111111000111",
                InscricaoEstadual = "123456789788",
                InscricaoMunicipal = "987654321",
                Endereco = "Rua A",
                Telefone = "(11) 1111-1111",
                Ativa = true
            }
        };

        _unidadeServiceMock
            .Setup(s => s.GetById(empresaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unidades);

        // Act
        var result = await _controller.GetById(empresaId, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<OsLogResponse<IEnumerable<UnidadeDto>>>(ok.Value);

        Assert.True(response.Sucesso);
        Assert.NotNull(response.Dados);
        Assert.Equal("Unidades retornadas com sucesso.", response.Mensagem);

        var dados = response.Dados!.ToList();
        Assert.Single(dados);
        Assert.Equal(empresaId, dados[0].EmpresaId);

        _unidadeServiceMock.Verify(
            s => s.GetById(empresaId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "[API-MOQ] GET ById Unidade - deve retornar NotFound quando lista estiver vazia")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "UnidadeController")]
    public async Task GetById_Deve_Retornar_NotFound_Quando_Lista_Estiver_Vazia()
    {
        // Arrange
        var empresaId = 1;

        _unidadeServiceMock
            .Setup(s => s.GetById(empresaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UnidadeDto>());

        // Act
        var result = await _controller.GetById(empresaId, CancellationToken.None);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<OsLogResponse<UnidadeCreateDto>>(notFound.Value);

        Assert.False(response.Sucesso);
        Assert.Null(response.Dados);

        _unidadeServiceMock.Verify(
            s => s.GetById(empresaId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // =========================================================
    // DELETE /api/unidades/{empresaId}/{unidadeId}
    // =========================================================

    [Fact(DisplayName = "[API-MOQ] DELETE Unidade - deve retornar NoContent quando excluir com sucesso")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "UnidadeController")]
    public async Task Delete_Deve_Retornar_NoContent_Quando_Excluir_Com_Sucesso()
    {
        // Arrange
        var empresaId = 1;
        var unidadeId = 2;

        _unidadeServiceMock
            .Setup(s => s.Delete(empresaId, unidadeId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(empresaId, unidadeId, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _unidadeServiceMock.Verify(
            s => s.Delete(empresaId, unidadeId, It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "[API-MOQ] DELETE Unidade - deve retornar NotFound quando unidade não existir")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "UnidadeController")]
    public async Task Delete_Deve_Retornar_NotFound_Quando_Unidade_Nao_Existir()
    {
        // Arrange
        var empresaId = 1;
        var unidadeId = 2;

        _unidadeServiceMock
            .Setup(s => s.Delete(empresaId, unidadeId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(empresaId, unidadeId, CancellationToken.None);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<OsLogResponse<object>>(notFound.Value);

        Assert.False(response.Sucesso);
        Assert.Null(response.Dados);

        _unidadeServiceMock.Verify(
            s => s.Delete(empresaId, unidadeId, It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}