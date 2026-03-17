using Microsoft.AspNetCore.Mvc;
using Moq;
using OsLog.API.Controllers;
using OsLog.Application.Common.Responses;
using OsLog.Application.DTOs.Empresa;
using OsLog.Application.Ports.ApplicationServices;

namespace OsLog.Tests.Unit.Moq.Api.Controllers;

public class EmpresaControllerMoqTests
{
    private readonly Mock<IEmpresaService> _empresaServiceMock;
    private readonly EmpresaController _empresaController;

    public EmpresaControllerMoqTests()
    {
        _empresaServiceMock = new Mock<IEmpresaService>(MockBehavior.Strict);
        _empresaController = new EmpresaController(_empresaServiceMock.Object);
    }

    // =========================================================
    // POST /api/empresas
    // =========================================================

    [Fact(DisplayName = "[API-MOQ] POST /api/empresas deve retornar CreatedAtAction com OsLogResponse.Ok")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task Create_Deve_Retornar_Created_Com_OsLogResponseOk()
    {
        // Arrange
        var dto = new EmpresaCreateDto
        {
            RazaoSocial = "Qualitech Assistência Técnica Ltda",
            NomeFantasia = "Qualitech",
            Cnpj = "12345678000199"
        };

        _empresaServiceMock
            .Setup(s => s.Create(dto, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(123);

        // Act
        var result = await _empresaController.Create(dto, CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(EmpresaController.GetById), created.ActionName);
        Assert.NotNull(created.RouteValues);
        Assert.Equal(123, created.RouteValues["id"]);

        var response = Assert.IsType<OsLogResponse<object>>(created.Value);
        Assert.True(response.Sucesso);
        Assert.Equal("Empresa criada com sucesso.", response.Mensagem);
        Assert.NotNull(response.Dados);
        Assert.Equal(123, response.Dados);

        _empresaServiceMock.Verify(
            s => s.Create(dto, It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "[API-MOQ] POST /api/empresas deve retornar ValidationProblem quando ModelState for inválido")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task Create_Deve_Retornar_ValidationProblem_Quando_ModelState_For_Invalido()
    {
        // Arrange
        var dto = new EmpresaCreateDto
        {
            RazaoSocial = "",
            NomeFantasia = "Qualitech",
            Cnpj = "12345678000199"
        };

        _empresaController.ModelState.AddModelError("RazaoSocial", "Razão social é obrigatória.");

        // Act
        var result = await _empresaController.Create(dto, CancellationToken.None);

        // Assert
        Assert.IsAssignableFrom<IActionResult>(result);

        _empresaServiceMock.Verify(
            s => s.Create(It.IsAny<EmpresaCreateDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // =========================================================
    // GET /api/empresas
    // =========================================================

    [Fact(DisplayName = "[API-MOQ] GET /api/empresas deve retornar Ok com lista")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task GetAll_Deve_Retornar_Ok_Com_Lista()
    {
        // Arrange
        var lista = new List<EmpresaListDto>
        {
            new()
            {
                Id = 1,
                RazaoSocial = "Empresa A Ltda",
                NomeFantasia = "Empresa A",
                Cnpj = "11111111000111",
                Ativa = true
            },
            new()
            {
                Id = 2,
                RazaoSocial = "Empresa B Ltda",
                NomeFantasia = "Empresa B",
                Cnpj = "22222222000122",
                Ativa = true
            }
        };

        _empresaServiceMock
            .Setup(s => s.GetAll(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lista);

        // Act
        var result = await _empresaController.GetAll(CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<OsLogResponse<IEnumerable<EmpresaListDto>>>(ok.Value);

        Assert.True(response.Sucesso);
        Assert.NotNull(response.Dados);
        Assert.Equal("Empresas retornadas com sucesso.", response.Mensagem);

        var dados = response.Dados!.ToList();
        Assert.Equal(2, dados.Count);
        Assert.Equal(1, dados[0].Id);
        Assert.Equal("Empresa A Ltda", dados[0].RazaoSocial);
        Assert.Equal(2, dados[1].Id);
        Assert.Equal("Empresa B Ltda", dados[1].RazaoSocial);

        _empresaServiceMock.Verify(
            s => s.GetAll(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "[API-MOQ] GET /api/empresas deve retornar NotFound quando lista estiver vazia")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task GetAll_Deve_Retornar_NotFound_Quando_Lista_Estiver_Vazia()
    {
        // Arrange
        _empresaServiceMock
            .Setup(s => s.GetAll(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmpresaListDto>());

        // Act
        var result = await _empresaController.GetAll(CancellationToken.None);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<OsLogResponse<EmpresaDetailDto>>(notFound.Value);

        Assert.False(response.Sucesso);

        _empresaServiceMock.Verify(
            s => s.GetAll(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // =========================================================
    // GET /api/empresas/{id}
    // =========================================================

    [Fact(DisplayName = "[API-MOQ] GET /api/empresas/{id} deve retornar Ok com empresa encontrada")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task GetById_Deve_Retornar_Ok_Quando_Empresa_Encontrada()
    {
        // Arrange
        var empresa = new EmpresaDetailDto
        {
            Id = 1,
            RazaoSocial = "Empresa A Ltda",
            NomeFantasia = "Empresa A",
            Cnpj = "11111111000111",
            DataCriacaoUtc = new DateTime(2025, 1, 10),
            Ativa = true
        };

        _empresaServiceMock
            .Setup(s => s.GetById(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(empresa);

        // Act
        var result = await _empresaController.GetById(1, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<OsLogResponse<EmpresaDetailDto>>(ok.Value);

        Assert.True(response.Sucesso);
        Assert.NotNull(response.Dados);
        Assert.Equal("Empresa encontrada.", response.Mensagem);
        Assert.Equal(1, response.Dados!.Id);
        Assert.Equal("Empresa A Ltda", response.Dados.RazaoSocial);
        Assert.Equal("Empresa A", response.Dados.NomeFantasia);
        Assert.Equal("11111111000111", response.Dados.Cnpj);
        Assert.True(response.Dados.Ativa);

        _empresaServiceMock.Verify(
            s => s.GetById(1, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "[API-MOQ] GET /api/empresas/{id} deve retornar NotFound quando empresa não existir")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task GetById_Deve_Retornar_NotFound_Quando_Empresa_Nao_Existir()
    {
        // Arrange
        _empresaServiceMock
            .Setup(s => s.GetById(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmpresaDetailDto?)null);

        // Act
        var result = await _empresaController.GetById(1, CancellationToken.None);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<OsLogResponse<EmpresaDetailDto>>(notFound.Value);

        Assert.False(response.Sucesso);

        _empresaServiceMock.Verify(
            s => s.GetById(1, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // =========================================================
    // DELETE /api/empresas/{id}
    // =========================================================

    [Fact(DisplayName = "[API-MOQ] DELETE /api/empresas/{id} deve retornar NoContent quando excluir com sucesso")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task Delete_Deve_Retornar_NoContent_Quando_Excluir_Com_Sucesso()
    {
        // Arrange
        _empresaServiceMock
            .Setup(s => s.Delete(1, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _empresaController.Delete(1, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _empresaServiceMock.Verify(
            s => s.Delete(1, It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "[API-MOQ] DELETE /api/empresas/{id} deve retornar NotFound quando empresa não existir")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task Delete_Deve_Retornar_NotFound_Quando_Empresa_Nao_Existir()
    {
        // Arrange
        _empresaServiceMock
            .Setup(s => s.Delete(1, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _empresaController.Delete(1, CancellationToken.None);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<OsLogResponse<object>>(notFound.Value);

        Assert.False(response.Sucesso);

        _empresaServiceMock.Verify(
            s => s.Delete(1, It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}