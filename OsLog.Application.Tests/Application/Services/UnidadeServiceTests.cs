using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using OsLog.Application.Common;
using OsLog.Application.DTOs.Empresa;
using OsLog.Application.Services;
using OsLog.Domain.Entities;
using Xunit;

namespace OsLog.Tests.Application.Services;

public class UnidadeServiceTests
{
    private readonly Mock<IUnitOfWork> _UnitOfWorkMoq = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly UnidadeService _service;

    public UnidadeServiceTests()
    {
        _service = new UnidadeService(_UnitOfWorkMoq.Object, _mapperMock.Object);
    }

    [Fact(DisplayName = "[Application] UnidadeService.ListarPorEmpresaAsync deve retornar lista mapeada")]
    [Trait("Category", "Application")]
    [Trait("SubCategory", "UnidadeService")]
    public async Task ListarPorEmpresaAsync_Deve_Retornar_Lista()
    {
        // Arrange
        var unidades = new List<Unidade>
        {
            new()
            {
                Id = 1,
                EmpresaId = 10,
                Nome = "Matriz",
                FlExcluido = false
            }
        };

        var dtos = new List<UnidadeDto>
        {
            new()
            {
                Id = 1,
                EmpresaId = 10,
                Nome = "Matriz",
                Ativa = true
            }
        };

        _UnitOfWorkMoq.Setup(u => u.Unidades.ListAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Unidade, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(unidades);

        _mapperMock.Setup(m => m.Map<List<UnidadeDto>>(unidades))
            .Returns(dtos);

        // Act
        var result = await _service.ListarPorEmpresaAsync(10, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(10, result[0].EmpresaId);
    }

    [Fact(DisplayName = "[Application] UnidadeService.CriarUnidadeAsync deve lançar se empresa nao existir")]
    [Trait("Category", "Application")]
    [Trait("SubCategory", "UnidadeService")]
    public async Task CriarUnidadeAsync_Deve_Lancar_Se_Empresa_Nao_Encontrada()
    {
        // Arrange
        _UnitOfWorkMoq.Setup(u => u.Empresas.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Empresa?)null);

        var dto = new UnidadeCreateDto
        {
            Nome = "Filial X"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CriarUnidadeAsync(10, dto, usuarioId: 99, CancellationToken.None));
    }

    [Fact(DisplayName = "[Application] UnidadeService.CriarUnidadeAsync deve lançar se empresa estiver excluida")]
    [Trait("Category", "Application")]
    [Trait("SubCategory", "UnidadeService")]
    public async Task CriarUnidadeAsync_Deve_Lancar_Se_Empresa_Excluida()
    {
        // Arrange
        var empresa = new Empresa
        {
            Id = 10,
            FlExcluido = true
        };

        _UnitOfWorkMoq.Setup(u => u.Empresas.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(empresa);

        var dto = new UnidadeCreateDto
        {
            Nome = "Filial X"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CriarUnidadeAsync(10, dto, usuarioId: 99, CancellationToken.None));
    }

    [Fact(DisplayName = "[Application] UnidadeService.CriarUnidadeAsync deve criar unidade vinculada à empresa")]
    [Trait("Category", "Application")]
    [Trait("SubCategory", "UnidadeService")]
    public async Task CriarUnidadeAsync_Deve_Criar_Unidade()
    {
        // Arrange
        var empresa = new Empresa
        {
            Id = 10,
            FlExcluido = false
        };

        _UnitOfWorkMoq.Setup(u => u.Empresas.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(empresa);

        Unidade? unidadeCapturada = null;

        _UnitOfWorkMoq.Setup(u => u.Unidades.AddAsync(It.IsAny<Unidade>(), It.IsAny<CancellationToken>()))
            .Callback<Unidade, CancellationToken>((u, _) =>
            {
                u.Id = 123;
                unidadeCapturada = u;
            })
            .Returns(Task.CompletedTask);

        _UnitOfWorkMoq.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var dto = new UnidadeCreateDto
        {
            Nome = "Lapa",
            Cnpj = "12.345.678/0002-70",
            Endereco = "Av. Teste, 100",
            Telefone = "(11) 99999-0000"
        };

        // Act
        var id = await _service.CriarUnidadeAsync(10, dto, usuarioId: 99, CancellationToken.None);

        // Assert
        Assert.Equal(123, id);
        Assert.NotNull(unidadeCapturada);
        Assert.Equal(10, unidadeCapturada!.EmpresaId);
        Assert.Equal("Lapa", unidadeCapturada.Nome);
        Assert.Equal(99, unidadeCapturada.AlteradoPor);
    }

    [Fact(DisplayName = "[Application] UnidadeService.SoftDeleteAsync deve retornar false se unidade nao encontrada")]
    [Trait("Category", "Application")]
    [Trait("SubCategory", "UnidadeService")]
    public async Task SoftDeleteAsync_Deve_Retornar_False_Quando_Nao_Encontrada()
    {
        // Arrange
        _UnitOfWorkMoq.Setup(u => u.Unidades.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Unidade?)null);

        // Act
        var ok = await _service.SoftDeleteAsync(1, 99, CancellationToken.None);

        // Assert
        Assert.False(ok);
        _UnitOfWorkMoq.Verify(u => u.Unidades.Update(It.IsAny<Unidade>()), Times.Never);
    }

    [Fact(DisplayName = "[Application] UnidadeService.SoftDeleteAsync deve retornar false se ja excluida")]
    [Trait("Category", "Application")]
    [Trait("SubCategory", "UnidadeService")]
    public async Task SoftDeleteAsync_Deve_Retornar_False_Quando_Ja_Excluida()
    {
        // Arrange
        var unidade = new Unidade
        {
            Id = 1,
            FlExcluido = true
        };

        _UnitOfWorkMoq.Setup(u => u.Unidades.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unidade);

        // Act
        var ok = await _service.SoftDeleteAsync(1, 99, CancellationToken.None);

        // Assert
        Assert.False(ok);
        _UnitOfWorkMoq.Verify(u => u.Unidades.Update(It.IsAny<Unidade>()), Times.Never);
    }

    [Fact(DisplayName = "[Application] UnidadeService.SoftDeleteAsync deve marcar como excluida e atualizar")]
    [Trait("Category", "Application")]
    [Trait("SubCategory", "UnidadeService")]
    public async Task SoftDeleteAsync_Deve_Excluir_Logicamente()
    {
        // Arrange
        var unidade = new Unidade
        {
            Id = 1,
            FlExcluido = false
        };

        _UnitOfWorkMoq.Setup(u => u.Unidades.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unidade);

        _UnitOfWorkMoq.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var ok = await _service.SoftDeleteAsync(1, 99, CancellationToken.None);

        // Assert
        Assert.True(ok);
        Assert.True(unidade.FlExcluido);
        Assert.Equal(99, unidade.AlteradoPor);
        _UnitOfWorkMoq.Verify(u => u.Unidades.Update(unidade), Times.Once);
        _UnitOfWorkMoq.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
