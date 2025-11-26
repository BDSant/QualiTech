using AutoMapper;
using Moq;
using OsLog.Application.Common;
using OsLog.Application.DTOs.Empresa;
using OsLog.Application.Services;
using OsLog.Domain.Entities;

namespace OsLog.Tests.Application.Services;

public class EmpresaServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly EmpresaService _service;

    public EmpresaServiceTests()
    {
        _service = new EmpresaService(_uowMock.Object, _mapperMock.Object);
    }

    [Fact(DisplayName = "[Application] EmpresaService.ListarAsync deve retornar lista mapeada")]
    [Trait("Category", "Application")]
    [Trait("SubCategory", "EmpresaService")]
    public async Task ListarAsync_Deve_Retornar_Lista()
    {
        // Arrange
        var empresas = new List<Empresa>
        {
            new()
            {
                Id = 1,
                RazaoSocial = "ConsertaSmart ME",
                NomeFantasia = "ConsertaSmart",
                Cnpj = "12.345.678/0001-99",
                FlExcluido = false
            }
        };

        var dtos = new List<EmpresaListDto>
        {
            new()
            {
                Id = 1,
                RazaoSocial = "ConsertaSmart ME",
                NomeFantasia = "ConsertaSmart",
                Cnpj = "12.345.678/0001-99",
                Ativa = true
            }
        };

        _uowMock.Setup(u => u.Empresas.ListAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Empresa, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(empresas);

        _mapperMock.Setup(m => m.Map<List<EmpresaListDto>>(empresas))
            .Returns(dtos);

        // Act
        var result = await _service.ListarAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }

    [Fact(DisplayName = "[Application] EmpresaService.ObterPorIdAsync deve retornar null se não encontrada")]
    [Trait("Category", "Application")]
    [Trait("SubCategory", "EmpresaService")]
    public async Task ObterPorIdAsync_Deve_Retornar_Null_Quando_Nao_Encontrada()
    {
        // Arrange
        _uowMock.Setup(u => u.Empresas.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Empresa?)null);

        // Act
        var result = await _service.ObterPorIdAsync(1, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact(DisplayName = "[Application] EmpresaService.ObterPorIdAsync deve retornar null se empresa excluida")]
    [Trait("Category", "Application")]
    [Trait("SubCategory", "EmpresaService")]
    public async Task ObterPorIdAsync_Deve_Retornar_Null_Quando_Excluida()
    {
        // Arrange
        var empresa = new Empresa
        {
            Id = 1,
            RazaoSocial = "X",
            NomeFantasia = "Y",
            FlExcluido = true
        };

        _uowMock.Setup(u => u.Empresas.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(empresa);

        // Act
        var result = await _service.ObterPorIdAsync(1, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact(DisplayName = "[Application] EmpresaService.ObterPorIdAsync deve retornar detalhe quando ativa")]
    [Trait("Category", "Application")]
    [Trait("SubCategory", "EmpresaService")]
    public async Task ObterPorIdAsync_Deve_Retornar_Detalhe_Quando_Ativa()
    {
        // Arrange
        var empresa = new Empresa
        {
            Id = 1,
            RazaoSocial = "ConsertaSmart ME",
            NomeFantasia = "ConsertaSmart",
            Cnpj = "12.345.678/0001-99",
            FlExcluido = false
        };

        var dto = new EmpresaDetailDto
        {
            Id = 1,
            RazaoSocial = empresa.RazaoSocial,
            NomeFantasia = empresa.NomeFantasia,
            Cnpj = empresa.Cnpj,
            Ativa = true
        };

        _uowMock.Setup(u => u.Empresas.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(empresa);

        _mapperMock.Setup(m => m.Map<EmpresaDetailDto>(empresa))
            .Returns(dto);

        // Act
        var result = await _service.ObterPorIdAsync(1, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.True(result.Ativa);
    }

    [Fact(DisplayName = "[Application] EmpresaService.CriarEmpresaAsync deve criar empresa e unidade matriz")]
    [Trait("Category", "Application")]
    [Trait("SubCategory", "EmpresaService")]
    public async Task CriarEmpresaAsync_Deve_Criar_Empresa_E_Unidade_Matriz()
    {
        // Arrange
        var dto = new EmpresaCreateDto
        {
            RazaoSocial = "ConsertaSmart ME",
            NomeFantasia = "ConsertaSmart",
            Cnpj = "12312.345.678/0001-99"
        };

        Empresa? empresaCapturada = null;
        Unidade? unidadeCapturada = null;

        _uowMock.Setup(u => u.Empresas.AddAsync(It.IsAny<Empresa>(), It.IsAny<CancellationToken>()))
            .Callback<Empresa, CancellationToken>((e, _) =>
            {
                e.Id = 10;
                empresaCapturada = e;
            })
            .Returns(Task.CompletedTask);

        _uowMock.Setup(u => u.Unidades.AddAsync(It.IsAny<Unidade>(), It.IsAny<CancellationToken>()))
            .Callback<Unidade, CancellationToken>((u, _) =>
            {
                unidadeCapturada = u;
                u.Id = 20;
            })
            .Returns(Task.CompletedTask);

        _uowMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var id = await _service.CriarEmpresaAsync(dto, usuarioId: 99, CancellationToken.None);

        // Assert
        Assert.Equal(10, id);
        Assert.NotNull(empresaCapturada);
        Assert.NotNull(unidadeCapturada);
        Assert.Equal("Matriz", unidadeCapturada!.Nome);
        Assert.Equal(empresaCapturada!.Cnpj, unidadeCapturada.Cnpj);
    }

    [Fact(DisplayName = "[Application] EmpresaService.SoftDeleteAsync deve retornar false se empresa nao encontrada")]
    [Trait("Category", "Application")]
    [Trait("SubCategory", "EmpresaService")]
    public async Task SoftDeleteAsync_Deve_Retornar_False_Quando_Nao_Encontrada()
    {
        // Arrange
        _uowMock.Setup(u => u.Empresas.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Empresa?)null);

        // Act
        var ok = await _service.SoftDeleteAsync(1, 99, CancellationToken.None);

        // Assert
        Assert.False(ok);
        _uowMock.Verify(u => u.Empresas.Update(It.IsAny<Empresa>()), Times.Never);
    }

    [Fact(DisplayName = "[Application] EmpresaService.SoftDeleteAsync deve retornar false se ja excluida")]
    [Trait("Category", "Application")]
    [Trait("SubCategory", "EmpresaService")]
    public async Task SoftDeleteAsync_Deve_Retornar_False_Quando_Ja_Excluida()
    {
        // Arrange
        var empresa = new Empresa
        {
            Id = 1,
            FlExcluido = true
        };

        _uowMock.Setup(u => u.Empresas.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(empresa);

        // Act
        var ok = await _service.SoftDeleteAsync(1, 99, CancellationToken.None);

        // Assert
        Assert.False(ok);
        _uowMock.Verify(u => u.Empresas.Update(It.IsAny<Empresa>()), Times.Never);
    }

    [Fact(DisplayName = "[Application] EmpresaService.SoftDeleteAsync deve marcar como excluida e atualizar")]
    [Trait("Category", "Application")]
    [Trait("SubCategory", "EmpresaService")]
    public async Task SoftDeleteAsync_Deve_Excluir_Logicamente()
    {
        // Arrange
        var empresa = new Empresa
        {
            Id = 1,
            FlExcluido = false
        };

        _uowMock.Setup(u => u.Empresas.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(empresa);

        _uowMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var ok = await _service.SoftDeleteAsync(1, 99, CancellationToken.None);

        // Assert
        Assert.True(ok);
        Assert.True(empresa.FlExcluido);
        Assert.Equal(99, empresa.AlteradoPor);
        _uowMock.Verify(u => u.Empresas.Update(empresa), Times.Once);
        _uowMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
