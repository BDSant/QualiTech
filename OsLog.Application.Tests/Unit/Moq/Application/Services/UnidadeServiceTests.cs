//using AutoMapper;
//using Moq;
//using OsLog.Application.Common;
//using OsLog.Application.DTOs.Unidade;
//using OsLog.Application.Services;
//using OsLog.Domain.Entities;

//namespace OsLog.Tests.Application.Services;

//public class UnidadeServiceTests
//{
//    private readonly Mock<IUnitOfWork> _UnitOfWorkMoq = new();
//    private readonly Mock<IMapper> _mapperMock = new();
//    private readonly UnidadeService _service;

//    public UnidadeServiceTests()
//    {
//        _service = new UnidadeService(_UnitOfWorkMoq.Object, _mapperMock.Object);
//    }

//    [Fact(DisplayName = "[Application] UnidadeService.ListarPorEmpresaAsync deve retornar lista mapeada")]
//    [Trait("Category", "Application")]
//    [Trait("SubCategory", "UnidadeService")]
//    public async Task ListarPorEmpresaAsync_Deve_Retornar_Lista()
//    {
//        // Arrange
//        var unidades = new List<Unidade>
//        {
//            new()
//            {
//                Id = 1,
//                EmpresaId = 10,
//                Nome = "Matriz",
//                FlExcluido = false
//            }
//        };

//        var dtos = new List<UnidadeDto>
//        {
//            new()
//            {
//                Id = 1,
//                EmpresaId = 10,
//                Nome = "Matriz",
//                Ativa = true
//            }
//        };

//        _UnitOfWorkMoq.Setup(u => u.Unidades.GetById(It.IsAny<System.Linq.Expressions.Expression<Func<Unidade, bool>>>(), It.IsAny<CancellationToken>()))
//            .ReturnsAsync(unidades);

//        _mapperMock.Setup(m => m.Map<List<UnidadeDto>>(unidades))
//            .Returns(dtos);

//        // Act
//        var result = await _service.GetById(10, CancellationToken.None);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Single(result);
//        Assert.Equal(10, result[0].EmpresaId);
//    }

//    [Fact(DisplayName = "[Application] UnidadeService.CriarUnidadeAsync deve lançar se empresa nao existir")]
//    [Trait("Category", "Application")]
//    [Trait("SubCategory", "UnidadeService")]
//    public async Task CriarUnidadeAsync_Deve_Lancar_Se_Empresa_Nao_Encontrada()
//    {
//        // Arrange
//        _UnitOfWorkMoq.Setup(u => u.Empresas.GetById(10, It.IsAny<CancellationToken>()))
//            .ReturnsAsync((Empresa?)null);

//        var dto = new UnidadeCreateDto
//        {
//            Nome = "Filial X"
//        };

//        // Act & Assert
//        await Assert.ThrowsAsync<InvalidOperationException>(() =>
//            _service.Create(10, dto, usuarioId: 99, CancellationToken.None));
//    }

//    [Fact(DisplayName = "[Application] UnidadeService.CriarUnidadeAsync deve lançar se empresa estiver excluida")]
//    [Trait("Category", "Application")]
//    [Trait("SubCategory", "UnidadeService")]
//    public async Task CriarUnidadeAsync_Deve_Lancar_Se_Empresa_Excluida()
//    {
//        // Arrange
//        var empresa = new Empresa
//        {
//            Id = 10,
//            FlExcluido = true
//        };

//        _UnitOfWorkMoq.Setup(u => u.Empresas.GetById(10, It.IsAny<CancellationToken>()))
//            .ReturnsAsync(empresa);

//        var dto = new UnidadeCreateDto
//        {
//            Nome = "Filial X"
//        };

//        // Act & Assert
//        await Assert.ThrowsAsync<InvalidOperationException>(() =>
//            _service.Create(10, dto, usuarioId: 99, CancellationToken.None));
//    }

//    [Fact(DisplayName = "[Application] UnidadeService.CriarUnidadeAsync deve criar unidade vinculada à empresa")]
//    [Trait("Category", "Application")]
//    [Trait("SubCategory", "UnidadeService")]
//    public async Task CriarUnidadeAsync_Deve_Criar_Unidade()
//    {
//        // Arrange
//        var empresa = new Empresa
//        {
//            Id = 10,
//            FlExcluido = false
//        };

//        _UnitOfWorkMoq.Setup(u => u.Empresas.GetById(10, It.IsAny<CancellationToken>()))
//            .ReturnsAsync(empresa);

//        Unidade? unidadeCapturada = null;

//        _UnitOfWorkMoq.Setup(u => u.Unidades.AddAsync(It.IsAny<Unidade>(), It.IsAny<CancellationToken>()))
//            .Callback<Unidade, CancellationToken>((u, _) =>
//            {
//                u.Id = 123;
//                unidadeCapturada = u;
//            })
//            .Returns(Task.CompletedTask);

//        _UnitOfWorkMoq.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
//            .ReturnsAsync(1);

//        var dto = new UnidadeCreateDto
//        {
//            Nome = "Lapa",
//            Cnpj = "12.345.678/0002-70",
//            Endereco = "Av. Teste, 100",
//            Telefone = "(11) 99999-0000"
//        };

//        // Act
//        var id = await _service.Create(10, dto, usuarioId: 99, CancellationToken.None);

//        // Assert
//        Assert.Equal(123, id);
//        Assert.NotNull(unidadeCapturada);
//        Assert.Equal(10, unidadeCapturada!.EmpresaId);
//        Assert.Equal("Lapa", unidadeCapturada.Nome);
//        Assert.Equal(99, unidadeCapturada.AlteradoPor);
//    }

//    [Fact(DisplayName = "[Application] UnidadeService.SoftDeleteAsync deve retornar false se unidade nao encontrada")]
//    [Trait("Category", "Application")]
//    [Trait("SubCategory", "UnidadeService")]
//    public async Task SoftDeleteAsync_Deve_Retornar_False_Quando_Nao_Encontrada()
//    {
//        // Arrange
//        _UnitOfWorkMoq.Setup(u => u.Unidades.GetById(1, It.IsAny<CancellationToken>()))
//            .ReturnsAsync((Unidade?)null);

//        // Act
//        var ok = await _service.Delete(1, 99, 1, CancellationToken.None);

//        // Assert
//        Assert.False(ok);
//        _UnitOfWorkMoq.Verify(u => u.Unidades.Update(It.IsAny<Unidade>()), Times.Never);
//    }

//    [Fact(DisplayName = "[Application] UnidadeService.SoftDeleteAsync deve retornar false se ja excluida")]
//    [Trait("Category", "Application")]
//    [Trait("SubCategory", "UnidadeService")]
//    public async Task SoftDeleteAsync_Deve_Retornar_False_Quando_Ja_Excluida()
//    {
//        // Arrange
//        var unidade = new Unidade
//        {
//            Id = 1,
//            FlExcluido = true
//        };

//        _UnitOfWorkMoq.Setup(u => u.Unidades.GetById(1, It.IsAny<CancellationToken>()))
//            .ReturnsAsync(unidade);

//        // Act
//        var ok = await _service.Delete(1, 99, 1, CancellationToken.None);

//        // Assert
//        Assert.False(ok);
//        _UnitOfWorkMoq.Verify(u => u.Unidades.Update(It.IsAny<Unidade>()), Times.Never);
//    }

//    [Fact(DisplayName = "[Application] UnidadeService.SoftDeleteAsync deve marcar como excluida e atualizar")]
//    [Trait("Category", "Application")]
//    [Trait("SubCategory", "UnidadeService")]
//    public async Task SoftDeleteAsync_Deve_Excluir_Logicamente()
//    {
//        // Arrange
//        var unidade = new Unidade
//        {
//            Id = 1,
//            FlExcluido = false
//        };

//        _UnitOfWorkMoq.Setup(u => u.Unidades.GetById(1, It.IsAny<CancellationToken>()))
//            .ReturnsAsync(unidade);

//        _UnitOfWorkMoq.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
//            .ReturnsAsync(1);

//        // Act
//        var ok = await _service.Delete(1, 99, 1, CancellationToken.None);

//        // Assert
//        Assert.True(ok);
//        Assert.True(unidade.FlExcluido);
//        Assert.Equal(99, unidade.AlteradoPor);
//        _UnitOfWorkMoq.Verify(u => u.Unidades.Update(unidade), Times.Once);
//        _UnitOfWorkMoq.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
//    }
//}




using AutoMapper;
using Moq;
using OsLog.Application.Common;
using OsLog.Application.DTOs.Unidade;
using OsLog.Application.Services;
using OsLog.Domain.Entities;
using OsLog.Application.Ports.Persistence.Repositories;
using Xunit;

namespace OsLog.Tests.Unit.Moq.Application.Services;

public class UnidadeServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IEmpresaRepository> _empresaRepositoryMock;
    private readonly Mock<IUnidadeRepository> _unidadeRepositoryMock;
    private readonly UnidadeService _service;

    public UnidadeServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _empresaRepositoryMock = new Mock<IEmpresaRepository>();
        _unidadeRepositoryMock = new Mock<IUnidadeRepository>();

        _unitOfWorkMock.SetupGet(x => x.Empresas)
            .Returns(_empresaRepositoryMock.Object);

        _unitOfWorkMock.SetupGet(x => x.Unidades)
            .Returns(_unidadeRepositoryMock.Object);

        _service = new UnidadeService(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Create_DeveCriarUnidade_QuandoEmpresaExistirEEstiverAtiva()
    {
        // Arrange
        var empresaId = 1;
        var usuarioId = 99;
        var ct = CancellationToken.None;

        var empresa = new Empresa
        {
            Id = empresaId,
            FlExcluido = false
        };

        var dto = new UnidadeCreateDto
        {
            Nome = "Unidade Centro",
            Cnpj = "12345678000199",
            InscricaoEstadual = "123456",
            InscricaoMunicipal = "654321",
            Endereco = "Rua A",
            Telefone = "11999999999"
        };

        Unidade? unidadeCapturada = null;

        _empresaRepositoryMock
            .Setup(x => x.GetById(empresaId, ct))
            .ReturnsAsync(empresa);

        _unidadeRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Unidade>(), ct))
            .Callback<Unidade, CancellationToken>((u, _) =>
            {
                unidadeCapturada = u;
                u.Id = 10;
            })
            .Returns(Task.CompletedTask);


        //    _uowMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
        //.ReturnsAsync(1);

        _unitOfWorkMock
            .Setup(x => x.CommitAsync(ct))
            .ReturnsAsync(It.IsAny<int>());//((Task<int>)Task.CompletedTask);

        // Act
        var result = await _service.Create(empresaId, dto, usuarioId, ct);

        // Assert
        Assert.Equal(10, result);

        Assert.NotNull(unidadeCapturada);
        Assert.Equal(empresaId, unidadeCapturada!.EmpresaId);
        Assert.Equal(dto.Nome, unidadeCapturada.Nome);
        Assert.Equal(dto.Cnpj, unidadeCapturada.Cnpj);
        Assert.Equal(dto.InscricaoEstadual, unidadeCapturada.InscricaoEstadual);
        Assert.Equal(dto.InscricaoMunicipal, unidadeCapturada.InscricaoMunicipal);
        Assert.Equal(dto.Endereco, unidadeCapturada.Endereco);
        Assert.Equal(dto.Telefone, unidadeCapturada.Telefone);
        Assert.Equal(usuarioId, unidadeCapturada.AlteradoPor);
        Assert.False(unidadeCapturada.FlExcluido);
        Assert.True(unidadeCapturada.DataCriacao <= DateTime.UtcNow);

        _empresaRepositoryMock.Verify(x => x.GetById(empresaId, ct), Times.Once);
        _unidadeRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Unidade>(), ct), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(ct), Times.Once);
    }

    [Fact]
    public async Task Create_DeveLancarInvalidOperationException_QuandoEmpresaNaoExistir()
    {
        // Arrange
        var empresaId = 1;
        var usuarioId = 99;
        var ct = CancellationToken.None;

        var dto = new UnidadeCreateDto
        {
            Nome = "Unidade Centro"
        };

        _empresaRepositoryMock
            .Setup(x => x.GetById(empresaId, ct))
            .ReturnsAsync((Empresa?)null);

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.Create(empresaId, dto, usuarioId, ct));

        // Assert
        Assert.Equal("Empresa não encontrada ou inativa.", ex.Message);

        _unidadeRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Unidade>(), ct), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(ct), Times.Never);
    }

    [Fact]
    public async Task Create_DeveLancarInvalidOperationException_QuandoEmpresaEstiverExcluida()
    {
        // Arrange
        var empresaId = 1;
        var usuarioId = 99;
        var ct = CancellationToken.None;

        var empresa = new Empresa
        {
            Id = empresaId,
            FlExcluido = true
        };

        var dto = new UnidadeCreateDto
        {
            Nome = "Unidade Centro"
        };

        _empresaRepositoryMock
            .Setup(x => x.GetById(empresaId, ct))
            .ReturnsAsync(empresa);

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.Create(empresaId, dto, usuarioId, ct));

        // Assert
        Assert.Equal("Empresa não encontrada ou inativa.", ex.Message);

        _unidadeRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Unidade>(), ct), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(ct), Times.Never);
    }

    [Fact]
    public async Task GetAll_DeveRetornarListaMapeada()
    {
        // Arrange
        var ct = CancellationToken.None;

        var unidades = new List<Unidade>
        {
            new() { Id = 1, EmpresaId = 1, Nome = "Unidade 1" },
            new() { Id = 2, EmpresaId = 1, Nome = "Unidade 2" }
        };

        var unidadesDto = new List<UnidadeDto>
        {
            new() { Id = 1, EmpresaId = 1, Nome = "Unidade 1" },
            new() { Id = 2, EmpresaId = 1, Nome = "Unidade 2" }
        };

        _unidadeRepositoryMock
            .Setup(x => x.GetAll(ct))
            .ReturnsAsync(unidades);

        _mapperMock
            .Setup(x => x.Map<List<UnidadeDto>>(unidades))
            .Returns(unidadesDto);

        // Act
        var result = await _service.GetAll(ct);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Unidade 1", result[0].Nome);
        Assert.Equal("Unidade 2", result[1].Nome);

        _unidadeRepositoryMock.Verify(x => x.GetAll(ct), Times.Once);
        _mapperMock.Verify(x => x.Map<List<UnidadeDto>>(unidades), Times.Once);
    }

    [Fact]
    public async Task GetById_DeveRetornarListaMapeadaDaEmpresa()
    {
        // Arrange
        var empresaId = 1;
        var ct = CancellationToken.None;

        var unidades = new List<Unidade>
        {
            new() { Id = 1, EmpresaId = empresaId, Nome = "Unidade Centro", FlExcluido = false }
        };

        var unidadesDto = new List<UnidadeDto>
        {
            new() { Id = 1, EmpresaId = empresaId, Nome = "Unidade Centro" }
        };

        _unidadeRepositoryMock
            .Setup(x => x.GetById(It.IsAny<System.Linq.Expressions.Expression<Func<Unidade, bool>>>(), ct))
            .ReturnsAsync(unidades);

        _mapperMock
            .Setup(x => x.Map<List<UnidadeDto>>(unidades))
            .Returns(unidadesDto);

        // Act
        var result = await _service.GetById(empresaId, ct);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(empresaId, result[0].EmpresaId);
        Assert.Equal("Unidade Centro", result[0].Nome);

        _unidadeRepositoryMock.Verify(
            x => x.GetById(It.IsAny<System.Linq.Expressions.Expression<Func<Unidade, bool>>>(), ct),
            Times.Once);

        _mapperMock.Verify(x => x.Map<List<UnidadeDto>>(unidades), Times.Once);
    }

    [Fact]
    public async Task Delete_DeveRetornarFalse_QuandoUnidadeNaoExistir()
    {
        // Arrange
        var empresaId = 1;
        var unidadeId = 10;
        var usuarioId = 99;
        var ct = CancellationToken.None;

        _unidadeRepositoryMock
            .Setup(x => x.GetById(It.IsAny<System.Linq.Expressions.Expression<Func<Unidade, bool>>>(), ct))
            .ReturnsAsync(new List<Unidade>());

        // Act
        var result = await _service.Delete(empresaId, unidadeId, usuarioId, ct);

        // Assert
        Assert.False(result);

        _unidadeRepositoryMock.Verify(x => x.Update(It.IsAny<Unidade>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(ct), Times.Never);
    }

    [Fact]
    public async Task Delete_DeveRetornarFalse_QuandoUnidadeJaEstiverExcluida()
    {
        // Arrange
        var empresaId = 1;
        var unidadeId = 10;
        var usuarioId = 99;
        var ct = CancellationToken.None;

        var unidade = new Unidade
        {
            Id = unidadeId,
            EmpresaId = empresaId,
            Nome = "Unidade A",
            FlExcluido = true
        };

        _unidadeRepositoryMock
            .Setup(x => x.GetById(It.IsAny<System.Linq.Expressions.Expression<Func<Unidade, bool>>>(), ct))
            .ReturnsAsync(new List<Unidade> { unidade });

        // Act
        var result = await _service.Delete(empresaId, unidadeId, usuarioId, ct);

        // Assert
        Assert.False(result);

        _unidadeRepositoryMock.Verify(x => x.Update(It.IsAny<Unidade>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(ct), Times.Never);
    }

    [Fact]
    public async Task Delete_DeveFazerSoftDelete_QuandoUnidadeExistirEEstiverAtiva()
    {
        // Arrange
        var empresaId = 1;
        var unidadeId = 10;
        var usuarioId = 99;
        var ct = CancellationToken.None;

        var unidade = new Unidade
        {
            Id = unidadeId,
            EmpresaId = empresaId,
            Nome = "Unidade A",
            FlExcluido = false
        };

        _unidadeRepositoryMock
            .Setup(x => x.GetById(It.IsAny<System.Linq.Expressions.Expression<Func<Unidade, bool>>>(), ct))
            .ReturnsAsync(new List<Unidade> { unidade });

        _unitOfWorkMock
            .Setup(x => x.CommitAsync(ct))
            .ReturnsAsync(It.IsAny<int>());

        // Act
        var result = await _service.Delete(empresaId, unidadeId, usuarioId, ct);

        // Assert
        Assert.True(result);
        Assert.True(unidade.FlExcluido);
        Assert.Equal(usuarioId, unidade.AlteradoPor);
        Assert.NotNull(unidade.DataAlteracao);

        _unidadeRepositoryMock.Verify(x => x.Update(unidade), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(ct), Times.Once);
    }
}