using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OsLog.API.Controllers;
using OsLog.Application.Common.Responses;
using OsLog.Application.DTOs.Empresa;
using OsLog.Application.Interfaces.Services;

namespace OsLog.Tests.Moq.API;

public class EmpresaControllerMoqTests
{
    private readonly Mock<IEmpresaService> _empresaServiceMock;
    private readonly Mock<IUnidadeService> _unidadeServiceMock;
    private readonly EmpresaController _controller;

    public EmpresaControllerMoqTests()
    {
        _empresaServiceMock = new Mock<IEmpresaService>(MockBehavior.Strict);
        _unidadeServiceMock = new Mock<IUnidadeService>(MockBehavior.Strict);

        _controller = new EmpresaController(
            _empresaServiceMock.Object,
            _unidadeServiceMock.Object);
    }

    // =========================================================
    // GET /api/empresas
    // =========================================================
    [Fact(DisplayName = "[API-MOQ] GET /api/empresas deve retornar OsLogResponse.Ok com lista")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task GetAll_Deve_Retornar_Ok_Com_Lista()
    {
        // Arrange
        var empresas = new List<EmpresaListDto>
        {
            new()
            {
                Id = 1,
                RazaoSocial = "ConsertaSmart ME",
                NomeFantasia = "ConsertaSmart Centro",
                Cnpj = "11111111000111"
            },
            new()
            {
                Id = 2,
                RazaoSocial = "ConsertaSmart ME - Lapa",
                NomeFantasia = "ConsertaSmart Lapa",
                Cnpj = "22222222000122"
            }
        };

        _empresaServiceMock
            .Setup(s => s.ListarAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(empresas);

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<OsLogResponse<IEnumerable<EmpresaListDto>>>(ok.Value);

        Assert.True(response.Sucesso);
        Assert.NotNull(response.Dados);

        var dados = new List<EmpresaListDto>(response.Dados!);
        Assert.Equal(2, dados.Count);
        Assert.Equal(empresas[0].Id, dados[0].Id);
        Assert.Equal(empresas[1].Id, dados[1].Id);

        _empresaServiceMock.Verify(
            s => s.ListarAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // =========================================================
    // GET /api/empresas/{id} - NotFound
    // =========================================================
    [Fact(DisplayName = "[API-MOQ] GET /api/empresas/{id} deve retornar NotFound quando null")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task GetById_Deve_Retornar_NotFound_Quando_Null()
    {
        // Arrange
        _empresaServiceMock
            .Setup(s => s.ObterPorIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmpresaDetailDto?)null);

        // Act
        var result = await _controller.GetById(1, CancellationToken.None);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);

        var response = Assert.IsType<OsLogResponse<EmpresaDetailDto>>(notFound.Value);
        Assert.False(response.Sucesso);
        Assert.Null(response.Dados);

        _empresaServiceMock.Verify(
            s => s.ObterPorIdAsync(1, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // =========================================================
    // GET /api/empresas/{id} - Ok
    // =========================================================
    [Fact(DisplayName = "[API-MOQ] GET /api/empresas/{id} deve retornar Ok quando encontrada")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task GetById_Deve_Retornar_Ok_Quando_Encontrada()
    {
        // Arrange
        var dto = new EmpresaDetailDto
        {
            Id = 10,
            RazaoSocial = "ConsertaSmart ME",
            NomeFantasia = "ConsertaSmart",
            Cnpj = "12345678000199",
            Ativa = true
        };

        _empresaServiceMock
            .Setup(s => s.ObterPorIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        // Act
        var result = await _controller.GetById(10, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<OsLogResponse<EmpresaDetailDto>>(ok.Value);

        Assert.True(response.Sucesso);
        Assert.NotNull(response.Dados);
        Assert.Equal(10, response.Dados!.Id);

        _empresaServiceMock.Verify(
            s => s.ObterPorIdAsync(10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // =========================================================
    // POST /api/empresas - Created (fluxo feliz)
    // =========================================================
    [Fact(DisplayName = "[API-MOQ] POST /api/empresas deve retornar CreatedAtAction com OsLogResponse.Ok (fluxo feliz)")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task Create_Deve_Retornar_Created_Com_OsLogResponseOk()
    {
        // Arrange
        var dto = new EmpresaCreateDto
        {
            RazaoSocial = "ConsertaSmart ME",
            NomeFantasia = "ConsertaSmart",
            Cnpj = "12345678000199"
        };

        _empresaServiceMock
            .Setup(s => s.CriarEmpresaAsync(dto, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(123);

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(EmpresaController.GetById), created.ActionName);
        Assert.Equal(123, created.RouteValues!["id"]);

        var response = Assert.IsType<OsLogResponse<object>>(created.Value);
        Assert.True(response.Sucesso);
        Assert.NotNull(response.Dados);

        // payload anônimo { Id = 123 }
        var payload = response.Dados!;
        var propId = payload.GetType().GetProperty("Id");
        Assert.NotNull(propId);
        Assert.Equal(123, (int)propId!.GetValue(payload)!);

        _empresaServiceMock.Verify(
            s => s.CriarEmpresaAsync(dto, It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // =========================================================
    // DELETE /api/empresas/{id} - NotFound
    // =========================================================
    [Fact(DisplayName = "[API-MOQ] DELETE /api/empresas/{id} deve retornar NotFound quando SoftDeleteAsync = false")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task Delete_Deve_Retornar_NotFound_Quando_False()
    {
        // Arrange
        _empresaServiceMock
            .Setup(s => s.SoftDeleteAsync(1, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(1, CancellationToken.None);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);

        var response = Assert.IsType<OsLogResponse<object>>(notFound.Value);
        Assert.False(response.Sucesso);
        Assert.Null(response.Dados);

        _empresaServiceMock.Verify(
            s => s.SoftDeleteAsync(1, It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // =========================================================
    // DELETE /api/empresas/{id} - NoContent
    // =========================================================
    [Fact(DisplayName = "[API-MOQ] DELETE /api/empresas/{id} deve retornar NoContent quando SoftDeleteAsync = true")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task Delete_Deve_Retornar_NoContent_Quando_True()
    {
        // Arrange
        _empresaServiceMock
            .Setup(s => s.SoftDeleteAsync(1, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1, CancellationToken.None);

        // Assert
        var noContent = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContent.StatusCode);

        _empresaServiceMock.Verify(
            s => s.SoftDeleteAsync(1, It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // =========================================================
    // POST /api/empresas/{empresaId}/unidades - Created (fluxo feliz)
    // =========================================================
    [Fact(DisplayName = "[API-MOQ] POST /api/empresas/{empresaId}/unidades deve retornar CreatedAtAction com OsLogResponse.Ok")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task CriarUnidade_Deve_Retornar_Created_Com_OsLogResponseOk()
    {
        // Arrange
        var dto = new UnidadeCreateDto
        {
            Nome = "Lapa",
            Cnpj = "33333333000133"
        };

        _unidadeServiceMock
            .Setup(s => s.CriarUnidadeAsync(10, dto, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(555);

        // Act
        var result = await _controller.CriarUnidade(10, dto, CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(EmpresaController.ListarUnidades), created.ActionName);
        Assert.Equal(10, created.RouteValues!["empresaId"]);

        var response = Assert.IsType<OsLogResponse<object>>(created.Value);
        Assert.True(response.Sucesso);
        Assert.NotNull(response.Dados);

        var payload = response.Dados!;
        var propId = payload.GetType().GetProperty("Id");
        var propEmpresaId = payload.GetType().GetProperty("EmpresaId");

        Assert.NotNull(propId);
        Assert.NotNull(propEmpresaId);
        Assert.Equal(555, (int)propId!.GetValue(payload)!);
        Assert.Equal(10, (int)propEmpresaId!.GetValue(payload)!);

        _unidadeServiceMock.Verify(
            s => s.CriarUnidadeAsync(10, dto, It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // =========================================================
    // GET /api/empresas/{empresaId}/unidades
    // =========================================================
    [Fact(DisplayName = "[API-MOQ] GET /api/empresas/{empresaId}/unidades deve retornar OsLogResponse.Ok com lista")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task ListarUnidades_Deve_Retornar_Ok_Com_Lista()
    {
        // Arrange
        var unidades = new List<UnidadeDto>
        {
            new() { Id = 1, EmpresaId = 10, Nome = "Matriz", Ativa = true }
        };

        _unidadeServiceMock
            .Setup(s => s.ListarPorEmpresaAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unidades);

        // Act
        var result = await _controller.ListarUnidades(10, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<OsLogResponse<IEnumerable<UnidadeDto>>>(ok.Value);

        Assert.True(response.Sucesso);
        Assert.NotNull(response.Dados);

        var dados = new List<UnidadeDto>(response.Dados!);
        Assert.Single(dados);
        Assert.Equal(10, dados[0].EmpresaId);

        _unidadeServiceMock.Verify(
            s => s.ListarPorEmpresaAsync(10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // =========================================================
    // GET /api/unidades
    // =========================================================
    [Fact(DisplayName = "[API-MOQ] GET /api/unidades deve retornar OsLogResponse.Ok com lista")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task ListarTodasUnidades_Deve_Retornar_Ok_Com_Lista()
    {
        // Arrange
        var unidades = new List<UnidadeDto>
        {
            new() { Id = 1, EmpresaId = 10, Nome = "Matriz", Ativa = true }
        };

        _unidadeServiceMock
            .Setup(s => s.ListarTodasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(unidades);

        // Act
        var result = await _controller.ListarTodasUnidades(CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<OsLogResponse<IEnumerable<UnidadeDto>>>(ok.Value);

        Assert.True(response.Sucesso);
        Assert.NotNull(response.Dados);

        var dados = new List<UnidadeDto>(response.Dados!);
        Assert.Single(dados);
        Assert.Equal(1, dados[0].Id);

        _unidadeServiceMock.Verify(
            s => s.ListarTodasAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
