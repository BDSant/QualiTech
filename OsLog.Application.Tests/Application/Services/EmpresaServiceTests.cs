using AutoMapper;
using Moq;
using OsLog.Application.Common;
using OsLog.Application.DTOs.Empresa;
using OsLog.Application.Ports.Persistence.Repositories;
using OsLog.Application.Services;
using OsLog.Domain.Entities;
using OsLog.Domain.Enums;
using Xunit;

namespace OsLog.Tests.Unit.Application.Services;

public class EmpresaServiceTests
{
    [Fact(DisplayName = "[UNIT] EmpresaService.Create deve criar empresa, matriz e UsuarioAcesso do proprietário")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "EmpresaService")]
    public async Task Create_DeveCriarEmpresaMatrizEUsuarioAcessoDoProprietario()
    {
        var empresaIdGerado = Guid.NewGuid();
        const string usuarioId = "user-domain-id-123";

        Empresa? empresaCapturada = null;
        Unidade? unidadeCapturada = null;
        UsuarioAcesso? usuarioAcessoCapturado = null;

        var empresasRepo = new Mock<IEmpresaRepository>();
        empresasRepo.Setup(x => x.AddAsync(It.IsAny<Empresa>(), It.IsAny<CancellationToken>()))
            .Callback<Empresa, CancellationToken>((e, _) => empresaCapturada = e)
            .Returns(Task.CompletedTask);

        var unidadesRepo = new Mock<IUnidadeRepository>();
        unidadesRepo.Setup(x => x.AddAsync(It.IsAny<Unidade>(), It.IsAny<CancellationToken>()))
            .Callback<Unidade, CancellationToken>((u, _) => unidadeCapturada = u)
            .Returns(Task.CompletedTask);

        var usuarioAcessoRepo = new Mock<IUsuarioAcessoRepository>();
        usuarioAcessoRepo.Setup(x => x.AddAsync(It.IsAny<UsuarioAcesso>(), It.IsAny<CancellationToken>()))
            .Callback<UsuarioAcesso, CancellationToken>((ua, _) => usuarioAcessoCapturado = ua)
            .Returns(Task.CompletedTask);

        var mapper = new Mock<IMapper>();

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Empresas).Returns(empresasRepo.Object);
        uow.SetupGet(x => x.Unidades).Returns(unidadesRepo.Object);
        uow.SetupGet(x => x.UsuarioAcessos).Returns(usuarioAcessoRepo.Object);

        uow.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(_ =>
            {
                if (empresaCapturada is not null)
                    empresaCapturada.Id = empresaIdGerado;
            })
            .ReturnsAsync(1);

        uow.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = new EmpresaService(uow.Object, mapper.Object);

        var dto = new EmpresaCreateDto
        {
            RazaoSocial = "Carlos Assistência Técnica Ltda",
            NomeFantasia = "Carlos Assistência",
            CnpjMatriz = "12.345.678/0001-90",
            InscricaoEstadualMatriz = "123456789",
            InscricaoMunicipalMatriz = "987654321",
            EnderecoMatriz = "Rua das Oficinas, 100",
            TelefoneMatriz = "(11)99999-9999"
        };

        var idRetornado = await sut.Create(dto, usuarioId, CancellationToken.None);

        Assert.Equal(empresaIdGerado, idRetornado);

        Assert.NotNull(empresaCapturada);
        Assert.Equal("Carlos Assistência Técnica Ltda", empresaCapturada!.RazaoSocial);
        Assert.True(empresaCapturada.Ativa);

        Assert.NotNull(unidadeCapturada);
        Assert.Equal(empresaIdGerado, unidadeCapturada!.EmpresaId);
        Assert.Equal("Matriz", unidadeCapturada.Nome);
        Assert.Equal(TipoUnidade.Matriz, unidadeCapturada.Tipo);
        Assert.True(unidadeCapturada.Ativa);

        Assert.NotNull(usuarioAcessoCapturado);
        Assert.Equal(usuarioId, usuarioAcessoCapturado!.UsuarioId);
        Assert.Equal(empresaIdGerado, usuarioAcessoCapturado.EmpresaId);
        Assert.Null(usuarioAcessoCapturado.UnidadeId);
        Assert.Equal(EscopoAcesso.Empresa, usuarioAcessoCapturado.Escopo);
        Assert.Equal(PerfilAcesso.Proprietario, usuarioAcessoCapturado.Perfil);
        Assert.True(usuarioAcessoCapturado.Ativo);

        uow.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "[UNIT] EmpresaService.GetById deve retornar null quando usuário não possuir acesso")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "EmpresaService")]
    public async Task GetById_DeveRetornarNull_QuandoUsuarioNaoPossuirAcesso()
    {
        var empresaId = Guid.NewGuid();

        var usuarioAcessoRepo = new Mock<IUsuarioAcessoRepository>();
        usuarioAcessoRepo.Setup(x => x.ObterListaPorUserIdAsync("user-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.UsuarioAcessos).Returns(usuarioAcessoRepo.Object);

        var mapper = new Mock<IMapper>();
        var sut = new EmpresaService(uow.Object, mapper.Object);

        var result = await sut.GetById(empresaId, "user-123", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact(DisplayName = "[UNIT] EmpresaService.Delete deve marcar empresa como inativa quando usuário tiver acesso")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "EmpresaService")]
    public async Task Delete_DeveMarcarEmpresaComoInativa_QuandoUsuarioTiverAcesso()
    {
        var empresaId = Guid.NewGuid();
        var empresa = new Empresa
        {
            Id = empresaId,
            RazaoSocial = "Empresa X",
            NomeFantasia = "Fantasia X",
            Ativa = true,
            DataCriacaoUtc = DateTime.UtcNow
        };

        var acessos = new List<UsuarioAcesso>
        {
            new()
            {
                UsuarioId = "user-123",
                EmpresaId = empresaId,
                UnidadeId = null,
                Escopo = EscopoAcesso.Empresa,
                Perfil = PerfilAcesso.Proprietario,
                Ativo = true,
                DataCriacaoUtc = DateTime.UtcNow
            }
        };

        var empresasRepo = new Mock<IEmpresaRepository>();
        empresasRepo.Setup(x => x.GetByPredicateAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Empresa, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(empresa);

        var usuarioAcessoRepo = new Mock<IUsuarioAcessoRepository>();
        usuarioAcessoRepo.Setup(x => x.ObterListaPorUserIdAsync("user-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(acessos);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Empresas).Returns(empresasRepo.Object);
        uow.SetupGet(x => x.UsuarioAcessos).Returns(usuarioAcessoRepo.Object);
        uow.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        uow.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var mapper = new Mock<IMapper>();
        var sut = new EmpresaService(uow.Object, mapper.Object);

        var result = await sut.Delete(empresaId, "user-123", CancellationToken.None);

        Assert.True(result);
        Assert.False(empresa.Ativa);
        empresasRepo.Verify(x => x.Update(empresa), Times.Once);
        uow.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "[UNIT] EmpresaService.GetAll deve retornar vazio quando usuário não possuir acessos")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "EmpresaService")]
    public async Task GetAll_DeveRetornarVazio_QuandoUsuarioNaoPossuirAcessos()
    {
        var usuarioAcessoRepo = new Mock<IUsuarioAcessoRepository>();
        usuarioAcessoRepo.Setup(x => x.ObterListaPorUserIdAsync("user-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.UsuarioAcessos).Returns(usuarioAcessoRepo.Object);

        var mapper = new Mock<IMapper>();
        var sut = new EmpresaService(uow.Object, mapper.Object);

        var result = await sut.GetAll("user-123", CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact(DisplayName = "[UNIT] EmpresaService.GetAll deve retornar todas as empresas quando acesso for de plataforma")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "EmpresaService")]
    public async Task GetAll_DeveRetornarTodasAsEmpresas_QuandoAcessoForPlataforma()
    {
        var empresas = new List<Empresa>
    {
        new() { Id = Guid.NewGuid(), RazaoSocial = "Empresa A", NomeFantasia = "Fantasia A", Ativa = true, DataCriacaoUtc = DateTime.UtcNow },
        new() { Id = Guid.NewGuid(), RazaoSocial = "Empresa B", NomeFantasia = "Fantasia B", Ativa = true, DataCriacaoUtc = DateTime.UtcNow }
    };

        var listaDto = empresas.Select(x => new EmpresaListDto
        {
            Id = x.Id,
            RazaoSocial = x.RazaoSocial,
            NomeFantasia = x.NomeFantasia
        }).ToList();

        var usuarioAcessoRepo = new Mock<IUsuarioAcessoRepository>();
        usuarioAcessoRepo.Setup(x => x.ObterListaPorUserIdAsync("user-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UsuarioAcesso>
            {
            new()
            {
                UsuarioId = "user-123",
                Escopo = EscopoAcesso.Plataforma,
                Perfil = PerfilAcesso.Administrador,
                Ativo = true,
                DataCriacaoUtc = DateTime.UtcNow
            }
            });

        var empresasRepo = new Mock<IEmpresaRepository>();
        empresasRepo.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Empresa, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(empresas);

        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<IReadOnlyList<EmpresaListDto>>(empresas))
            .Returns(listaDto);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.UsuarioAcessos).Returns(usuarioAcessoRepo.Object);
        uow.SetupGet(x => x.Empresas).Returns(empresasRepo.Object);

        var sut = new EmpresaService(uow.Object, mapper.Object);

        var result = await sut.GetAll("user-123", CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact(DisplayName = "[UNIT] EmpresaService.GetById deve retornar empresa quando usuário possuir acesso por empresa")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "EmpresaService")]
    public async Task GetById_DeveRetornarEmpresa_QuandoUsuarioPossuirAcessoPorEmpresa()
    {
        var empresaId = Guid.NewGuid();

        var empresa = new Empresa
        {
            Id = empresaId,
            RazaoSocial = "Empresa X",
            NomeFantasia = "Fantasia X",
            Ativa = true,
            DataCriacaoUtc = DateTime.UtcNow
        };

        var dto = new EmpresaDetailDto
        {
            Id = empresaId,
            RazaoSocial = "Empresa X",
            NomeFantasia = "Fantasia X"
        };

        var usuarioAcessoRepo = new Mock<IUsuarioAcessoRepository>();
        usuarioAcessoRepo.Setup(x => x.ObterListaPorUserIdAsync("user-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UsuarioAcesso>
            {
            new()
            {
                UsuarioId = "user-123",
                EmpresaId = empresaId,
                Escopo = EscopoAcesso.Empresa,
                Perfil = PerfilAcesso.Proprietario,
                Ativo = true,
                DataCriacaoUtc = DateTime.UtcNow
            }
            });

        var empresasRepo = new Mock<IEmpresaRepository>();
        empresasRepo.Setup(x => x.GetByPredicateAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Empresa, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(empresa);

        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<EmpresaDetailDto>(empresa)).Returns(dto);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.UsuarioAcessos).Returns(usuarioAcessoRepo.Object);
        uow.SetupGet(x => x.Empresas).Returns(empresasRepo.Object);

        var sut = new EmpresaService(uow.Object, mapper.Object);

        var result = await sut.GetById(empresaId, "user-123", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(empresaId, result!.Id);
    }

    [Fact(DisplayName = "[UNIT] EmpresaService.Delete deve retornar false quando usuário não possuir permissão")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "EmpresaService")]
    public async Task Delete_DeveRetornarFalse_QuandoUsuarioNaoPossuirPermissao()
    {
        var empresaId = Guid.NewGuid();

        var usuarioAcessoRepo = new Mock<IUsuarioAcessoRepository>();
        usuarioAcessoRepo.Setup(x => x.ObterListaPorUserIdAsync("user-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.UsuarioAcessos).Returns(usuarioAcessoRepo.Object);
        uow.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        uow.Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var mapper = new Mock<IMapper>();
        var sut = new EmpresaService(uow.Object, mapper.Object);

        var result = await sut.Delete(empresaId, "user-123", CancellationToken.None);

        Assert.False(result);
        uow.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}