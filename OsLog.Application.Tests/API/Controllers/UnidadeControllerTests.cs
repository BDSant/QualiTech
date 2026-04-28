using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using OsLog.API.Controllers;
using OsLog.Application.DTOs.Empresa;
using OsLog.Application.DTOs.Unidade;
using OsLog.Application.Ports.ApplicationServices;
using System.Security.Claims;
using Xunit;

namespace OsLog.Tests.Unit.API.Controllers;

public class UnidadeControllerTests
{
    [Fact(DisplayName = "[UNIT] EmpresaController.Create deve retornar CreatedAtAction quando cadastro for válido")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task Create_DeveRetornarCreatedAtAction_QuandoCadastroForValido()
    {
        var service = new Mock<IEmpresaService>();
        var empresaId = Guid.NewGuid();

        service.Setup(x => x.Create(It.IsAny<EmpresaCreateDto>(), "user-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(empresaId);

        var controller = CreateController(service, BuildUser("user-123"), isDevelopment: false);

        var dto = new EmpresaCreateDto
        {
            RazaoSocial = "Empresa Teste",
            NomeFantasia = "Fantasia Teste",
            CnpjMatriz = "12.345.678/0001-90",
            InscricaoEstadualMatriz = "123",
            InscricaoMunicipalMatriz = "456",
            EnderecoMatriz = "Rua A",
            TelefoneMatriz = "11999999999"
        };

        var result = await controller.Create(dto, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("GetById", created.ActionName);
        Assert.Equal(empresaId, created.RouteValues!["id"]);
    }

    [Fact(DisplayName = "[UNIT] UnidadeController.GetById deve retornar NotFound quando unidade não for encontrada")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "UnidadeController")]
    public async Task GetById_DeveRetornarNotFound_QuandoUnidadeNaoForEncontrada()
    {
        var empresaId = Guid.NewGuid();
        var service = new Mock<IUnidadeService>();

        service.Setup(x => x.GetById(10, empresaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UnidadeDto?)null);

        var controller = CreateController(service.Object);

        var result = await controller.GetById(empresaId, 10, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact(DisplayName = "[UNIT] UnidadeController.Delete deve retornar NoContent quando exclusão lógica for realizada")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "UnidadeController")]
    public async Task Delete_DeveRetornarNoContent_QuandoExclusaoLogicaForRealizada()
    {
        var empresaId = Guid.NewGuid();
        var service = new Mock<IUnidadeService>();

        service.Setup(x => x.Delete(10, empresaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = CreateController(service.Object);

        var result = await controller.Delete(empresaId, 10, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact(DisplayName = "[UNIT] UnidadeController.Create deve retornar NotFound quando empresa não existir")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "UnidadeController")]
    public async Task Create_DeveRetornarNotFound_QuandoEmpresaNaoExistir()
    {
        var empresaId = Guid.NewGuid();
        var service = new Mock<IUnidadeService>();

        service.Setup(x => x.Create(empresaId, It.IsAny<UnidadeCreateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var controller = CreateController(service.Object);

        var dto = new UnidadeCreateDto
        {
            Nome = "Filial 1",
            Cnpj = "12.345.678/0002-90"
        };

        var result = await controller.Create(empresaId, dto, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact(DisplayName = "[UNIT] UnidadeController.GetAll deve retornar NotFound quando lista estiver vazia")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "UnidadeController")]
    public async Task GetAll_DeveRetornarNotFound_QuandoListaEstiverVazia()
    {
        var empresaId = Guid.NewGuid();
        var service = new Mock<IUnidadeService>();

        service.Setup(x => x.GetAllByEmpresa(empresaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<UnidadeDto>());

        var controller = CreateController(service.Object);

        var result = await controller.GetAll(empresaId, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact(DisplayName = "[UNIT] UnidadeController.GetAll deve retornar Ok quando houver unidades")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "UnidadeController")]
    public async Task GetAll_DeveRetornarOk_QuandoHouverUnidades()
    {
        var empresaId = Guid.NewGuid();
        var lista = new List<UnidadeDto>
    {
        new()
        {
            Id = 1,
            Nome = "Filial 1",
            Cnpj = "12.345.678/0002-90"
        }
    };

        var service = new Mock<IUnidadeService>();
        service.Setup(x => x.GetAllByEmpresa(empresaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lista);

        var controller = CreateController(service.Object);

        var result = await controller.GetAll(empresaId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact(DisplayName = "[UNIT] UnidadeController.Delete deve retornar NotFound quando exclusão falhar")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "UnidadeController")]
    public async Task Delete_DeveRetornarNotFound_QuandoExclusaoFalhar()
    {
        var empresaId = Guid.NewGuid();
        var service = new Mock<IUnidadeService>();

        service.Setup(x => x.Delete(10, empresaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var controller = CreateController(service.Object);

        var result = await controller.Delete(empresaId, 10, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    private static UnidadeController CreateController(IUnidadeService service)
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };

        return new UnidadeController(service)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    private static EmpresaController CreateController(
        Mock<IEmpresaService> service,
        ClaimsPrincipal? user = null,
        bool isDevelopment = false)
    {
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(x => x.EnvironmentName)
            .Returns(isDevelopment ? Environments.Development : Environments.Production);

        var provider = new ServiceCollection()
            .AddSingleton(environment.Object)
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = provider,
            User = user ?? new ClaimsPrincipal(new ClaimsIdentity())
        };

        return new EmpresaController(service.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    private static ClaimsPrincipal BuildUser(string userId)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId)
        ], "TestAuth");

        return new ClaimsPrincipal(identity);
    }

}