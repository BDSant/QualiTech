using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OsLog.Api.Controllers;
using OsLog.Application.DTOs.Empresa;
using OsLog.Application.Interfaces.Services;
using OsLog.Application.Services;
using Xunit;

namespace OsLog.Tests.Api.Moq;

public class EmpresaControllerMoqTests
{
    private readonly Mock<IEmpresaService> _empresaServiceMock;
    private readonly Mock<IUnidadeService> _unidadeServiceMock;
    private readonly EmpresaController _controller;

    public EmpresaControllerMoqTests()
    {
        _empresaServiceMock = new Mock<IEmpresaService>(MockBehavior.Strict);
        _unidadeServiceMock = new Mock<IUnidadeService>(MockBehavior.Strict);

        _controller = new EmpresaController(_empresaServiceMock.Object, _unidadeServiceMock.Object);
    }

    [Fact(DisplayName = "[API-MOQ] GET /api/empresas deve retornar lista")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task GetAll_Deve_Retornar_Ok_Com_Lista()
    {
        // Arrange
        var lista = new List<EmpresaListDto>
        {
            new() { Id = 1, RazaoSocial = "ConsertaSmart ME", NomeFantasia = "ConsertaSmart", Cnpj = "12.345.678/0001-99", Ativa = true }
        };

        _empresaServiceMock.Setup(s => s.ListarAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lista);

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsAssignableFrom<IEnumerable<EmpresaListDto>>(ok.Value);
        Assert.Single(value);
    }

    [Fact(DisplayName = "[API-MOQ] GET /api/empresas/{id} deve retornar NotFound quando null")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task GetById_Deve_Retornar_NotFound_Quando_Null()
    {
        // Arrange
        _empresaServiceMock.Setup(s => s.ObterPorIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmpresaDetailDto?)null);

        // Act
        var result = await _controller.GetById(1, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact(DisplayName = "[API-MOQ] GET /api/empresas/{id} deve retornar Ok quando existir")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task GetById_Deve_Retornar_Ok_Quando_Encontrada()
    {
        // Arrange
        var dto = new EmpresaDetailDto
        {
            Id = 1,
            RazaoSocial = "ConsertaSmart ME",
            NomeFantasia = "ConsertaSmart",
            Cnpj = "12.345.678/0001-99",
            Ativa = true
        };

        _empresaServiceMock.Setup(s => s.ObterPorIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        // Act
        var result = await _controller.GetById(1, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsType<EmpresaDetailDto>(ok.Value);
        Assert.Equal(1, value.Id);
    }

    [Fact(DisplayName = "[API-MOQ] POST /api/empresas deve retornar ValidationProblem quando ModelState invalido")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task Create_Deve_Retornar_ValidationProblem_Quando_ModelState_Invalido()
    {
        // Arrange
        var dto = new EmpresaCreateDto(); // sem preencher para forçar erro

        // Força o ModelState a ser inválido
        _controller.ModelState.AddModelError("RazaoSocial", "Campo obrigatório");

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        // 1) Garante que voltamos um ObjectResult (ValidationProblem)
        var objectResult = Assert.IsType<ObjectResult>(result);

        // 2) Garante que o corpo é um ValidationProblemDetails
        var details = Assert.IsType<ValidationProblemDetails>(objectResult.Value);
        Assert.True(details.Errors.ContainsKey("RazaoSocial"));

        // ⚠ Não vamos mais depender de StatusCode aqui, pois fora do pipeline ele pode ser null
        // Se quiser só garantir que é erro de validação:
        // Assert.True(details.Title?.Contains("validation", StringComparison.OrdinalIgnoreCase) ?? true);
    }


    [Fact(DisplayName = "[API-MOQ] POST /api/empresas deve retornar CreatedAtAction")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task Create_Deve_Retornar_Created()
    {
        // Arrange
        var dto = new EmpresaCreateDto
        {
            RazaoSocial = "ConsertaSmart ME",
            NomeFantasia = "ConsertaSmart",
            Cnpj = "12.345.678/0001-99"
        };

        _empresaServiceMock.Setup(s => s.CriarEmpresaAsync(dto, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(EmpresaController.GetById), created.ActionName);
        Assert.Equal(10, created.RouteValues!["id"]);
    }

    [Fact(DisplayName = "[API-MOQ] DELETE /api/empresas/{id} deve retornar NotFound quando false")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task Delete_Deve_Retornar_NotFound_Quando_False()
    {
        // Arrange
        _empresaServiceMock.Setup(s => s.SoftDeleteAsync(1, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(1, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact(DisplayName = "[API-MOQ] DELETE /api/empresas/{id} deve retornar NoContent quando true")]
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
        // Em teste unitário, StatusCode pode ser null, então não vamos forçar 204 aqui

        _empresaServiceMock.Verify(s => s.SoftDeleteAsync(
            1,
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }


    //[Fact(DisplayName = "[API-MOQ] DELETE /api/empresas/{id} deve retornar NoContent quando true")]
    //[Trait("Category", "Api.Moq")]
    //[Trait("SubCategory", "EmpresaController")]
    //public async Task Delete_Deve_Retornar_NoContent_Quando_True()
    //{
    //    // Arrange
    //    _empresaServiceMock.Setup(s => s.SoftDeleteAsync(1, It.IsAny<int>(), It.IsAny<CancellationToken>()))
    //        .ReturnsAsync(true);

    //    // Act
    //    var result = await _controller.Delete(1, CancellationToken.None);

    //    // Assert
    //    Assert.IsType<NoContentResult>(result);
    //}

    [Fact(DisplayName = "[API-MOQ] GET /api/empresas/{empresaId}/unidades deve retornar lista")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task ListarUnidades_Deve_Retornar_Ok_Com_Lista()
    {
        // Arrange
        var lista = new List<UnidadeDto>
        {
            new() { Id = 1, EmpresaId = 10, Nome = "Matriz", Ativa = true }
        };

        _unidadeServiceMock.Setup(s => s.ListarPorEmpresaAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lista);

        // Act
        var result = await _controller.ListarUnidades(10, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsAssignableFrom<IEnumerable<UnidadeDto>>(ok.Value);
        Assert.Single(value);
    }

    [Fact(DisplayName = "[API-MOQ] GET /api/empresas/unidades deve retornar lista")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task ListarTodasUnidades_Deve_Retornar_Ok_Com_Lista()
    {
        // Arrange
        var lista = new List<UnidadeDto>
        {
            new() { Id = 1, EmpresaId = 10, Nome = "Matriz", Ativa = true }
        };

        _unidadeServiceMock.Setup(s => s.ListarTodasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lista);

        // Act
        var result = await _controller.ListarTodasUnidades(CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsAssignableFrom<IEnumerable<UnidadeDto>>(ok.Value);
        Assert.Single(value);
    }

    [Fact(DisplayName = "[API-MOQ] POST /api/empresas/{empresaId}/unidades deve retornar ValidationProblem quando ModelState invalido")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task CriarUnidade_Deve_Retornar_ValidationProblem_Quando_ModelState_Invalido()
    {
        // Arrange
        var dto = new UnidadeCreateDto(); // sem Nome, por exemplo

        _controller.ModelState.AddModelError("Nome", "Campo obrigatório");

        // Act
        var result = await _controller.CriarUnidade(empresaId: 1, dto, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        var details = Assert.IsType<ValidationProblemDetails>(objectResult.Value);
        Assert.True(details.Errors.ContainsKey("Nome"));
    }

    [Fact(DisplayName = "[API-MOQ] POST /api/empresas/{empresaId}/unidades deve retornar CreatedAtAction")]
    [Trait("Category", "Api.Moq")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task CriarUnidade_Deve_Retornar_Created()
    {
        // Arrange
        var dto = new UnidadeCreateDto
        {
            Nome = "Lapa"
        };

        _unidadeServiceMock.Setup(s => s.CriarUnidadeAsync(10, dto, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(123);

        // Act
        var result = await _controller.CriarUnidade(10, dto, CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(EmpresaController.ListarUnidades), created.ActionName);
        Assert.Equal(10, created.RouteValues!["empresaId"]);
    }
}
