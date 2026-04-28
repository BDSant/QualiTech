using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using OsLog.API.Controllers;
using OsLog.Application.DTOs.Empresa;
using OsLog.Application.Ports.ApplicationServices;
using System.Security.Claims;
using Xunit;

namespace OsLog.Tests.Unit.API.Controllers;

public class EmpresaControllerTests
{
    [Fact(DisplayName = "[UNIT] EmpresaController.Create deve retornar Unauthorized quando usuário não existir no token")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task Create_DeveRetornarUnauthorized_QuandoUsuarioNaoExistirNoToken()
    {
        var service = new Mock<IEmpresaService>();
        var controller = CreateController(service);

        var dto = new EmpresaCreateDto
        {
            RazaoSocial = "Empresa Teste",
            NomeFantasia = "Fantasia Teste",
            CnpjMatriz = "12.345.678/0001-90"
        };

        var result = await controller.Create(dto, CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result);
        service.Verify(x => x.Create(It.IsAny<EmpresaCreateDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "[UNIT] EmpresaController.Create deve retornar CreatedAtAction quando cadastro for válido")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task Create_DeveRetornarCreatedAtAction_QuandoCadastroForValido()
    {
        var service = new Mock<IEmpresaService>();
        var empresaId = Guid.NewGuid();

        service.Setup(x => x.Create(It.IsAny<EmpresaCreateDto>(), "user-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(empresaId);

        var controller = CreateController(service, BuildUser("user-123"));

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

    [Fact(DisplayName = "[UNIT] EmpresaController.GetById deve retornar NotFound quando empresa não for encontrada")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task GetById_DeveRetornarNotFound_QuandoEmpresaNaoForEncontrada()
    {
        var service = new Mock<IEmpresaService>();
        var empresaId = Guid.NewGuid();

        service.Setup(x => x.GetById(empresaId, "user-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmpresaDetailDto?)null);

        var controller = CreateController(service, BuildUser("user-123"));

        var result = await controller.GetById(empresaId, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact(DisplayName = "[UNIT] EmpresaController.GetAll deve retornar Unauthorized quando usuário não existir no token")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task GetAll_DeveRetornarUnauthorized_QuandoUsuarioNaoExistirNoToken()
    {
        var service = new Mock<IEmpresaService>();
        var controller = CreateController(service);

        var result = await controller.GetAll(CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result);
        service.Verify(x => x.GetAll(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "[UNIT] EmpresaController.GetAll deve retornar NotFound quando lista estiver vazia")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task GetAll_DeveRetornarNotFound_QuandoListaEstiverVazia()
    {
        var service = new Mock<IEmpresaService>();

        service.Setup(x => x.GetAll("user-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<EmpresaListDto>());

        var controller = CreateController(service, BuildUser("user-123"));

        var result = await controller.GetAll(CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact(DisplayName = "[UNIT] EmpresaController.GetAll deve retornar Ok quando houver empresas")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task GetAll_DeveRetornarOk_QuandoHouverEmpresas()
    {
        var lista = new List<EmpresaListDto>
    {
        new()
        {
            Id = Guid.NewGuid(),
            RazaoSocial = "Empresa A",
            NomeFantasia = "Fantasia A"
        }
    };

        var service = new Mock<IEmpresaService>();

        service.Setup(x => x.GetAll("user-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(lista);

        var controller = CreateController(service, BuildUser("user-123"));

        var result = await controller.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact(DisplayName = "[UNIT] EmpresaController.GetById deve retornar Unauthorized quando usuário não existir no token")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task GetById_DeveRetornarUnauthorized_QuandoUsuarioNaoExistirNoToken()
    {
        var service = new Mock<IEmpresaService>();
        var controller = CreateController(service);

        var result = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result);
        service.Verify(x => x.GetById(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "[UNIT] EmpresaController.GetById deve retornar Ok quando empresa for encontrada")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task GetById_DeveRetornarOk_QuandoEmpresaForEncontrada()
    {
        var empresaId = Guid.NewGuid();

        var empresa = new EmpresaDetailDto
        {
            Id = empresaId,
            RazaoSocial = "Empresa X",
            NomeFantasia = "Fantasia X"
        };

        var service = new Mock<IEmpresaService>();

        service.Setup(x => x.GetById(empresaId, "user-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(empresa);

        var controller = CreateController(service, BuildUser("user-123"));

        var result = await controller.GetById(empresaId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact(DisplayName = "[UNIT] EmpresaController.Delete deve retornar Unauthorized quando usuário não existir no token")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task Delete_DeveRetornarUnauthorized_QuandoUsuarioNaoExistirNoToken()
    {
        var service = new Mock<IEmpresaService>();
        var controller = CreateController(service);

        var result = await controller.Delete(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result);
        service.Verify(x => x.Delete(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "[UNIT] EmpresaController.Delete deve retornar NotFound quando exclusão falhar")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task Delete_DeveRetornarNotFound_QuandoExclusaoFalhar()
    {
        var empresaId = Guid.NewGuid();
        var service = new Mock<IEmpresaService>();

        service.Setup(x => x.Delete(empresaId, "user-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var controller = CreateController(service, BuildUser("user-123"));

        var result = await controller.Delete(empresaId, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact(DisplayName = "[UNIT] EmpresaController.Delete deve retornar NoContent quando exclusão lógica for realizada")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "EmpresaController")]
    public async Task Delete_DeveRetornarNoContent_QuandoExclusaoLogicaForRealizada()
    {
        var empresaId = Guid.NewGuid();
        var service = new Mock<IEmpresaService>();

        service.Setup(x => x.Delete(empresaId, "user-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = CreateController(service, BuildUser("user-123"));

        var result = await controller.Delete(empresaId, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    private static EmpresaController CreateController(
        Mock<IEmpresaService> service,
        ClaimsPrincipal? user = null,
        bool isDevelopment = false)
    {
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupProperty(x => x.EnvironmentName, isDevelopment ? Environments.Development : Environments.Production);

        var provider = new ServiceCollection()
            .AddSingleton(environment.Object)
            .BuildServiceProvider();

        var controller = new EmpresaController(service.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = provider,
                    User = user ?? new ClaimsPrincipal(new ClaimsIdentity())
                }
            }
        };

        return controller;
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