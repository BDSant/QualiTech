using AutoMapper;
using Moq;
using OsLog.Application.Common;
using OsLog.Application.DTOs.Empresa;
using OsLog.Application.Ports.Persistence.Repositories;
using OsLog.Application.Services;
using OsLog.Domain.Entities;
using OsLog.Domain.Enums;
using OsLog.Domain.Interfaces.Repositories;

namespace OsLog.Tests.Unit.Application.Services;

public class EmpresaServiceTests
{
    [Fact(DisplayName = "[UNIT] Create deve criar empresa, matriz e UsuarioAcesso do proprietário")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "EmpresaService")]
    public async Task Create_DeveCriarEmpresaMatrizEUsuarioAcessoDoProprietario()
    {
        // Arrange
        var empresaIdGerado = Guid.NewGuid();
        const string usuarioId = "b3b6fbea-5869-495c-b794-fdef999cf998";

        Empresa? empresaCapturada = null;
        Unidade? unidadeCapturada = null;
        UsuarioAcesso? usuarioAcessoCapturado = null;

        var empresasRepo = new Mock<IEmpresaRepository>();
        empresasRepo
            .Setup(x => x.AddAsync(It.IsAny<Empresa>(), It.IsAny<CancellationToken>()))
            .Callback<Empresa, CancellationToken>((e, _) => empresaCapturada = e)
            .Returns(Task.CompletedTask);

        var unidadesRepo = new Mock<IUnidadeRepository>();
        unidadesRepo
            .Setup(x => x.AddAsync(It.IsAny<Unidade>(), It.IsAny<CancellationToken>()))
            .Callback<Unidade, CancellationToken>((u, _) => unidadeCapturada = u)
            .Returns(Task.CompletedTask);

        var usuarioAcessoRepo = new Mock<IUsuarioAcessoRepository>();
        usuarioAcessoRepo
            .Setup(x => x.AddAsync(It.IsAny<UsuarioAcesso>(), It.IsAny<CancellationToken>()))
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

        // Act
        var idRetornado = await sut.Create(dto, usuarioId, CancellationToken.None);

        // Assert
        Assert.Equal(empresaIdGerado, idRetornado);

        Assert.NotNull(empresaCapturada);
        Assert.Equal("Carlos Assistência Técnica Ltda", empresaCapturada!.RazaoSocial);
        Assert.Equal("Carlos Assistência", empresaCapturada.NomeFantasia);
        Assert.True(empresaCapturada.Ativa);

        Assert.NotNull(unidadeCapturada);
        Assert.Equal(empresaIdGerado, unidadeCapturada!.EmpresaId);
        Assert.Equal("Matriz", unidadeCapturada.Nome);
        Assert.Equal(dto.CnpjMatriz, unidadeCapturada.Cnpj);
        Assert.Equal(dto.InscricaoEstadualMatriz, unidadeCapturada.InscricaoEstadual);
        Assert.Equal(dto.InscricaoMunicipalMatriz, unidadeCapturada.InscricaoMunicipal);
        Assert.Equal(dto.EnderecoMatriz, unidadeCapturada.Endereco);
        Assert.Equal(dto.TelefoneMatriz, unidadeCapturada.Telefone);
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
}