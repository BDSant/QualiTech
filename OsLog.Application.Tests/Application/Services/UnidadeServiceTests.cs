using AutoMapper;
using Moq;
using OsLog.Application.Common;
using OsLog.Application.DTOs.Unidade;
using OsLog.Application.Ports.Persistence.Repositories;
using OsLog.Application.Services;
using OsLog.Domain.Entities;
using OsLog.Domain.Enums;
using Xunit;

namespace OsLog.Tests.Unit.Application.Services;

public class UnidadeServiceTests
{
    [Fact(DisplayName = "[UNIT] UnidadeService.Create deve criar filial quando empresa existir e estiver ativa")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "UnidadeService")]
    public async Task Create_DeveCriarFilial_QuandoEmpresaExistirEEstiverAtiva()
    {
        var empresaId = Guid.NewGuid();
        var unidadeCapturada = new Unidade();

        var empresa = new Empresa
        {
            Id = empresaId,
            RazaoSocial = "Empresa X",
            NomeFantasia = "Fantasia X",
            Ativa = true,
            DataCriacaoUtc = DateTime.UtcNow
        };

        var empresasRepo = new Mock<IEmpresaRepository>();
        empresasRepo.Setup(x => x.GetByPredicateAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Empresa, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(empresa);

        var unidadesRepo = new Mock<IUnidadeRepository>();
        unidadesRepo.Setup(x => x.AddAsync(It.IsAny<Unidade>(), It.IsAny<CancellationToken>()))
            .Callback<Unidade, CancellationToken>((u, _) => unidadeCapturada = u)
            .Returns(Task.CompletedTask);

        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<Unidade>(It.IsAny<UnidadeCreateDto>()))
            .Returns(new Unidade
            {
                Nome = "Filial 1",
                Cnpj = "12.345.678/0002-90",
                InscricaoEstadual = "123",
                InscricaoMunicipal = "456",
                Endereco = "Rua B",
                Telefone = "11988888888"
            });

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Empresas).Returns(empresasRepo.Object);
        uow.SetupGet(x => x.Unidades).Returns(unidadesRepo.Object);
        uow.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(_ => unidadeCapturada.Id = 10)
            .ReturnsAsync(1);
        uow.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var sut = new UnidadeService(uow.Object, mapper.Object);

        var dto = new UnidadeCreateDto
        {
            Nome = "Filial 1",
            Cnpj = "12.345.678/0002-90",
            InscricaoEstadual = "123",
            InscricaoMunicipal = "456",
            Endereco = "Rua B",
            Telefone = "11988888888"
        };

        var result = await sut.Create(empresaId, dto, CancellationToken.None);

        Assert.Equal(10, result);
        Assert.Equal(empresaId, unidadeCapturada.EmpresaId);
        Assert.Equal(TipoUnidade.Filial, unidadeCapturada.Tipo);
        Assert.True(unidadeCapturada.Ativa);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "[UNIT] UnidadeService.GetById deve retornar null quando unidade não existir")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "UnidadeService")]
    public async Task GetById_DeveRetornarNull_QuandoUnidadeNaoExistir()
    {
        var empresaId = Guid.NewGuid();

        var unidadesRepo = new Mock<IUnidadeRepository>();
        unidadesRepo.Setup(x => x.GetByPredicateAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Unidade, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Unidade?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Unidades).Returns(unidadesRepo.Object);

        var mapper = new Mock<IMapper>();
        var sut = new UnidadeService(uow.Object, mapper.Object);

        var result = await sut.GetById(10, empresaId, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact(DisplayName = "[UNIT] UnidadeService.Delete deve marcar unidade como inativa quando encontrada")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "UnidadeService")]
    public async Task Delete_DeveMarcarUnidadeComoInativa_QuandoEncontrada()
    {
        var empresaId = Guid.NewGuid();
        var unidade = new Unidade
        {
            Id = 10,
            EmpresaId = empresaId,
            Nome = "Filial 1",
            Cnpj = "12.345.678/0002-90",
            Tipo = TipoUnidade.Filial,
            Ativa = true,
            DataCriacaoUtc = DateTime.UtcNow
        };

        var unidadesRepo = new Mock<IUnidadeRepository>();
        unidadesRepo.Setup(x => x.GetByPredicateAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Unidade, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(unidade);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Unidades).Returns(unidadesRepo.Object);
        uow.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        uow.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var mapper = new Mock<IMapper>();
        var sut = new UnidadeService(uow.Object, mapper.Object);

        var result = await sut.Delete(10, empresaId, CancellationToken.None);

        Assert.True(result);
        Assert.False(unidade.Ativa);
        unidadesRepo.Verify(x => x.Update(unidade), Times.Once);
        uow.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "[UNIT] UnidadeService.Create deve retornar 0 quando empresa não existir")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "UnidadeService")]
    public async Task Create_DeveRetornarZero_QuandoEmpresaNaoExistir()
    {
        var empresaId = Guid.NewGuid();

        var empresasRepo = new Mock<IEmpresaRepository>();
        empresasRepo.Setup(x => x.GetByPredicateAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Empresa, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Empresa?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Empresas).Returns(empresasRepo.Object);
        uow.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        uow.Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var mapper = new Mock<IMapper>();
        var sut = new UnidadeService(uow.Object, mapper.Object);

        var dto = new UnidadeCreateDto
        {
            Nome = "Filial 1",
            Cnpj = "12.345.678/0002-90"
        };

        var result = await sut.Create(empresaId, dto, CancellationToken.None);

        Assert.Equal(0, result);
        uow.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "[UNIT] UnidadeService.GetAllByEmpresa deve retornar lista mapeada")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "UnidadeService")]
    public async Task GetAllByEmpresa_DeveRetornarListaMapeada()
    {
        var empresaId = Guid.NewGuid();

        var unidades = new List<Unidade>
    {
        new()
        {
            Id = 1,
            EmpresaId = empresaId,
            Nome = "Filial 1",
            Cnpj = "12.345.678/0002-90",
            Tipo = TipoUnidade.Filial,
            Ativa = true,
            DataCriacaoUtc = DateTime.UtcNow
        }
    };

        var listaDto = new List<UnidadeDto>
    {
        new()
        {
            Id = 1,
            Nome = "Filial 1",
            Cnpj = "12.345.678/0002-90"
        }
    };

        var unidadesRepo = new Mock<IUnidadeRepository>();
        unidadesRepo.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Unidade, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(unidades);

        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<IReadOnlyList<UnidadeDto>>(unidades))
            .Returns(listaDto);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Unidades).Returns(unidadesRepo.Object);

        var sut = new UnidadeService(uow.Object, mapper.Object);

        var result = await sut.GetAllByEmpresa(empresaId, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Filial 1", result[0].Nome);
    }

    [Fact(DisplayName = "[UNIT] UnidadeService.GetById deve retornar dto quando unidade existir")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "UnidadeService")]
    public async Task GetById_DeveRetornarDto_QuandoUnidadeExistir()
    {
        var empresaId = Guid.NewGuid();

        var unidade = new Unidade
        {
            Id = 10,
            EmpresaId = empresaId,
            Nome = "Filial 1",
            Cnpj = "12.345.678/0002-90",
            Tipo = TipoUnidade.Filial,
            Ativa = true,
            DataCriacaoUtc = DateTime.UtcNow
        };

        var dto = new UnidadeDto
        {
            Id = 10,
            Nome = "Filial 1",
            Cnpj = "12.345.678/0002-90"
        };

        var unidadesRepo = new Mock<IUnidadeRepository>();
        unidadesRepo.Setup(x => x.GetByPredicateAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Unidade, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(unidade);

        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<UnidadeDto>(unidade)).Returns(dto);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Unidades).Returns(unidadesRepo.Object);

        var sut = new UnidadeService(uow.Object, mapper.Object);

        var result = await sut.GetById(10, empresaId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(10, result!.Id);
    }

    [Fact(DisplayName = "[UNIT] UnidadeService.Delete deve retornar false quando unidade não for encontrada")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "UnidadeService")]
    public async Task Delete_DeveRetornarFalse_QuandoUnidadeNaoForEncontrada()
    {
        var empresaId = Guid.NewGuid();

        var unidadesRepo = new Mock<IUnidadeRepository>();
        unidadesRepo.Setup(x => x.GetByPredicateAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Unidade, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Unidade?)null);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Unidades).Returns(unidadesRepo.Object);
        uow.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        uow.Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var mapper = new Mock<IMapper>();
        var sut = new UnidadeService(uow.Object, mapper.Object);

        var result = await sut.Delete(10, empresaId, CancellationToken.None);

        Assert.False(result);
        uow.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}