//using AutoMapper;
//using Moq;
//using OsLog.Application.Common;
//using OsLog.Application.DTOs.Empresa;
//using OsLog.Application.Interfaces.Repositories;
//using OsLog.Application.Mapping;
//using OsLog.Application.Services;
//using OsLog.Domain.Entities;

//namespace OsLog.Tests.Moq.Services
//{
//    public class EmpresaServiceMoqTests
//    {
//        private readonly Mock<IUnitOfWork> _uowMock;
//        private readonly Mock<IEmpresaRepository> _empresaRepoMock;
//        private readonly IMapper _mapper;
//        private readonly EmpresaService _service;

//        public EmpresaServiceMoqTests()
//        {
//            _uowMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
//            _empresaRepoMock = new Mock<IEmpresaRepository>(MockBehavior.Strict);

//            // Sempre que o service acessar _uow.Empresas, devolve o mock do repositório
//            _uowMock
//                .SetupGet(u => u.Empresas)
//                .Returns(_empresaRepoMock.Object);

//            // CommitAsync é chamado em quase todos os métodos
//            _uowMock
//                .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
//                .ReturnsAsync(1);

//            var cfg = new MapperConfiguration(c =>
//            {
//                c.AddMaps(typeof(EmpresaProfile).Assembly);
//            });
//            _mapper = cfg.CreateMapper();

//            _service = new EmpresaService(_uowMock.Object, _mapper);
//        }

//        // =========================================================
//        // CriarEmpresaAsync
//        // =========================================================
//        [Fact(DisplayName = "[Service-MOQ] CriarEmpresaAsync deve persistir e retornar Id")]
//        [Trait("Category", "Service.Moq")]
//        [Trait("SubCategory", "EmpresaService")]
//        public async Task CriarEmpresaAsync_Deve_Persistir_E_Retornar_Id()
//        {
//            // Arrange
//            var dto = new EmpresaCreateDto
//            {
//                RazaoSocial = "ConsertaSmart ME",
//                NomeFantasia = "ConsertaSmart Centro",
//                Cnpj = "12345678000199"
//            };

//            // Simula que o AddAsync vai setar um Id > 0
//            _empresaRepoMock
//                .Setup(r => r.AddAsync(It.IsAny<Empresa>(), It.IsAny<CancellationToken>()))
//                .Callback<Empresa, CancellationToken>((e, _) =>
//                {
//                    e.Id = 123;
//                })
//                .Returns(Task.CompletedTask);

//            // CommitAsync já foi configurado no construtor

//            // Act
//            var id = await _service.CriarEmpresaAsync(dto, usuarioId: 1, CancellationToken.None);

//            // Assert
//            Assert.Equal(123, id);

//            _empresaRepoMock.Verify(
//                r => r.AddAsync(It.IsAny<Empresa>(), It.IsAny<CancellationToken>()),
//                Times.Once);

//            _uowMock.Verify(
//                u => u.CommitAsync(It.IsAny<CancellationToken>()),
//                Times.Once);
//        }

//        // =========================================================
//        // ListarAsync
//        // =========================================================
//        [Fact(DisplayName = "[Service-MOQ] ListarAsync deve retornar apenas empresas não excluídas")]
//        [Trait("Category", "Service.Moq")]
//        [Trait("SubCategory", "EmpresaService")]
//        public async Task ListarAsync_Deve_Retornar_Apenas_Ativas()
//        {
//            // Arrange
//            var entities = new List<Empresa>
//            {
//                new()
//                {
//                    Id = 1,
//                    RazaoSocial = "Ativa 1",
//                    NomeFantasia = "Ativa 1",
//                    Cnpj = "11111111111111",
//                    FlExcluido = false
//                },
//                new()
//                {
//                    Id = 2,
//                    RazaoSocial = "Excluída",
//                    NomeFantasia = "Excluída",
//                    Cnpj = "22222222222222",
//                    FlExcluido = true
//                }
//            };

//            _empresaRepoMock
//                .Setup(r => r.ListAsync(It.IsAny<CancellationToken>()))
//                .ReturnsAsync(entities);

//            // Act
//            var lista = await _service.ListarAsync(CancellationToken.None);

//            // Assert
//            Assert.Single(lista);
//            Assert.Equal(1, lista.First().Id);
//            Assert.Equal("Ativa 1", lista.First().RazaoSocial);

//            _empresaRepoMock.Verify(
//                r => r.ListAsync(It.IsAny<CancellationToken>()),
//                Times.Once);
//        }

//        // =========================================================
//        // ObterPorIdAsync - encontrado
//        // =========================================================
//        [Fact(DisplayName = "[Service-MOQ] ObterPorIdAsync deve retornar DTO quando existir e não estiver excluída")]
//        [Trait("Category", "Service.Moq")]
//        [Trait("SubCategory", "EmpresaService")]
//        public async Task ObterPorIdAsync_Deve_Retornar_Detalhe_Quando_Existir()
//        {
//            // Arrange
//            var entity = new Empresa
//            {
//                Id = 10,
//                RazaoSocial = "ConsertaSmart ME",
//                NomeFantasia = "ConsertaSmart",
//                Cnpj = "12345678000199",
//                FlExcluido = false
//            };

//            _empresaRepoMock
//                .Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(entity);

//            // Act
//            var dto = await _service.ObterPorIdAsync(10, CancellationToken.None);

//            // Assert
//            Assert.NotNull(dto);
//            Assert.Equal(10, dto!.Id);
//            Assert.Equal(entity.RazaoSocial, dto.RazaoSocial);

//            _empresaRepoMock.Verify(
//                r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()),
//                Times.Once);
//        }

//        // =========================================================
//        // ObterPorIdAsync - não encontrado ou excluída
//        // =========================================================
//        [Fact(DisplayName = "[Service-MOQ] ObterPorIdAsync deve retornar null quando não existir")]
//        [Trait("Category", "Service.Moq")]
//        [Trait("SubCategory", "EmpresaService")]
//        public async Task ObterPorIdAsync_Deve_Retornar_Null_Quando_Nao_Existir()
//        {
//            // Arrange
//            _empresaRepoMock
//                .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
//                .ReturnsAsync((Empresa?)null);

//            // Act
//            var dto = await _service.ObterPorIdAsync(999, CancellationToken.None);

//            // Assert
//            Assert.Null(dto);

//            _empresaRepoMock.Verify(
//                r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()),
//                Times.Once);
//        }

//        [Fact(DisplayName = "[Service-MOQ] ObterPorIdAsync deve retornar null quando FlExcluido = true")]
//        [Trait("Category", "Service.Moq")]
//        [Trait("SubCategory", "EmpresaService")]
//        public async Task ObterPorIdAsync_Deve_Retornar_Null_Quando_FlExcluido()
//        {
//            // Arrange
//            var entity = new Empresa
//            {
//                Id = 20,
//                RazaoSocial = "Excluída",
//                NomeFantasia = "X",
//                Cnpj = "00000000000000",
//                FlExcluido = true
//            };

//            _empresaRepoMock
//                .Setup(r => r.GetByIdAsync(20, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(entity);

//            // Act
//            var dto = await _service.ObterPorIdAsync(20, CancellationToken.None);

//            // Assert
//            Assert.Null(dto);

//            _empresaRepoMock.Verify(
//                r => r.GetByIdAsync(20, It.IsAny<CancellationToken>()),
//                Times.Once);
//        }

//        // =========================================================
//        // SoftDeleteAsync
//        // =========================================================
//        [Fact(DisplayName = "[Service-MOQ] SoftDeleteAsync deve marcar FlExcluido = true e chamar Update + Commit")]
//        [Trait("Category", "Service.Moq")]
//        [Trait("SubCategory", "EmpresaService")]
//        public async Task SoftDeleteAsync_Deve_Marcar_FlExcluido_Quando_Encontrada()
//        {
//            // Arrange
//            var entity = new Empresa
//            {
//                Id = 30,
//                RazaoSocial = "Empresa Ativa",
//                NomeFantasia = "Ativa",
//                Cnpj = "12345678000199",
//                FlExcluido = false
//            };

//            _empresaRepoMock
//                .Setup(r => r.GetByIdAsync(30, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(entity);

//            _empresaRepoMock
//                .Setup(r => r.Update(It.IsAny<Empresa>()));

//            // CommitAsync já configurado no ctor

//            // Act
//            var ok = await _service.SoftDeleteAsync(30, usuarioId: 99, CancellationToken.None);

//            // Assert
//            Assert.True(ok);
//            Assert.True(entity.FlExcluido);
//            Assert.Equal(99, entity.AlteradoPor);

//            _empresaRepoMock.Verify(
//                r => r.GetByIdAsync(30, It.IsAny<CancellationToken>()),
//                Times.Once);

//            _empresaRepoMock.Verify(
//                r => r.Update(It.IsAny<Empresa>()),
//                Times.Once);

//            _uowMock.Verify(
//                u => u.CommitAsync(It.IsAny<CancellationToken>()),
//                Times.Once);
//        }

//        [Fact(DisplayName = "[Service-MOQ] SoftDeleteAsync deve retornar false quando empresa não encontrada")]
//        [Trait("Category", "Service.Moq")]
//        [Trait("SubCategory", "EmpresaService")]
//        public async Task SoftDeleteAsync_Deve_Retornar_False_Quando_Nao_Encontrada()
//        {
//            // Arrange
//            _empresaRepoMock
//                .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
//                .ReturnsAsync((Empresa?)null);

//            // Act
//            var ok = await _service.SoftDeleteAsync(999, usuarioId: 1, CancellationToken.None);

//            // Assert
//            Assert.False(ok);

//            _empresaRepoMock.Verify(
//                r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()),
//                Times.Once);

//            // Não deve chamar Update nem Commit
//            _empresaRepoMock.Verify(
//                r => r.Update(It.IsAny<Empresa>()),
//                Times.Never);

//            _uowMock.Verify(
//                u => u.CommitAsync(It.IsAny<CancellationToken>()),
//                Times.Never);
//        }
//    }
//}
using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OsLog.Application.Common;                 // IUnitOfWork
using OsLog.Application.DTOs.Empresa;
using OsLog.Application.Interfaces.Repositories;
using OsLog.Application.Mapping;
using OsLog.Application.Services;
using OsLog.Domain.Entities;
using System.Linq.Expressions;

namespace OsLog.Tests.Moq.Services
{
    public class EmpresaServiceMoqTests
    {
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<IEmpresaRepository> _empresaRepoMock;
        private readonly Mock<IUnidadeRepository> _unidadeRepoMock;
        private readonly IMapper _mapper;
        private readonly EmpresaService _service;

        public EmpresaServiceMoqTests()
        {
            _uowMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

            _empresaRepoMock = new Mock<IEmpresaRepository>(MockBehavior.Strict);
            _unidadeRepoMock = new Mock<IUnidadeRepository>(MockBehavior.Strict);

            // Expor os repositórios via UnitOfWork
            _uowMock
                .SetupGet(u => u.Empresas)
                .Returns(_empresaRepoMock.Object);

            _uowMock
                .SetupGet(u => u.Unidades)
                .Returns(_unidadeRepoMock.Object);

            // CommitAsync padrão
            _uowMock
                .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // AutoMapper apenas com os perfis necessários
            var cfg = new MapperConfiguration(c =>
            {
                c.AddMaps(typeof(EmpresaProfile).Assembly);
            }, NullLoggerFactory.Instance);

            _mapper = cfg.CreateMapper();
            _service = new EmpresaService(_uowMock.Object, _mapper);
        }

        // =========================================================
        // CriarEmpresaAsync
        // =========================================================
        [Fact(DisplayName = "[Service-MOQ] CriarEmpresaAsync deve persistir empresa, criar unidade matriz e retornar Id")]
        [Trait("Category", "Service.Moq")]
        [Trait("SubCategory", "EmpresaService")]
        public async Task CriarEmpresaAsync_Deve_Persistir_E_Retornar_Id()
        {
            // Arrange
            var dto = new EmpresaCreateDto
            {
                RazaoSocial = "ConsertaSmart ME",
                NomeFantasia = "ConsertaSmart Centro",
                Cnpj = "12345678000199"
            };

            Empresa? empresaCriada = null;
            Unidade? unidadeCriada = null;

            // AddAsync da empresa
            _empresaRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Empresa>(), It.IsAny<CancellationToken>()))
                .Callback<Empresa, CancellationToken>((e, _) =>
                {
                    e.Id = 123;
                    empresaCriada = e;
                })
                .Returns(Task.CompletedTask);

            // AddAsync da unidade matriz
            _unidadeRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Unidade>(), It.IsAny<CancellationToken>()))
                .Callback<Unidade, CancellationToken>((u, _) =>
                {
                    unidadeCriada = u;
                })
                .Returns(Task.CompletedTask);

            // Act
            var id = await _service.CriarEmpresaAsync(dto, usuarioId: 1, CancellationToken.None);

            // Assert
            Assert.Equal(123, id);

            Assert.NotNull(empresaCriada);
            Assert.Equal(dto.RazaoSocial, empresaCriada!.RazaoSocial);
            Assert.Equal(dto.NomeFantasia, empresaCriada.NomeFantasia);
            Assert.Equal(dto.Cnpj, empresaCriada.Cnpj);

            Assert.NotNull(unidadeCriada);
            // Como o service seta Empresa (nav prop), não EmpresaId:
            Assert.Same(empresaCriada, unidadeCriada!.Empresa);
            Assert.Equal("Matriz", unidadeCriada.Nome);
            Assert.Equal(dto.Cnpj, unidadeCriada.Cnpj);

            _empresaRepoMock.Verify(
                r => r.AddAsync(It.IsAny<Empresa>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _unidadeRepoMock.Verify(
                r => r.AddAsync(It.IsAny<Unidade>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _uowMock.Verify(
                u => u.CommitAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // =========================================================
        // ListarAsync
        // =========================================================
        [Fact(DisplayName = "[Service-MOQ] ListarAsync deve usar filtro !FlExcluido e retornar apenas empresas ativas")]
        [Trait("Category", "Service.Moq")]
        [Trait("SubCategory", "EmpresaService")]
        public async Task ListarAsync_Deve_Retornar_Apenas_Ativas()
        {
            // Arrange
            var entities = new List<Empresa>
            {
                new()
                {
                    Id = 1,
                    RazaoSocial = "Ativa 1",
                    NomeFantasia = "Ativa 1",
                    Cnpj = "11111111111111",
                    FlExcluido = false
                },
                new()
                {
                    Id = 2,
                    RazaoSocial = "Excluída",
                    NomeFantasia = "Excluída",
                    Cnpj = "22222222222222",
                    FlExcluido = true
                }
            };

            // Setup correto do overload com predicate:
            _empresaRepoMock
                .Setup(r => r.ListAsync(
                    It.IsAny<Expression<Func<Empresa, bool>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Expression<Func<Empresa, bool>> predicate, CancellationToken _) =>
                {
                    var func = predicate.Compile();
                    return entities.Where(func).ToList();
                });

            // Act
            var lista = await _service.ListarAsync(CancellationToken.None);

            // Assert
            Assert.Single(lista);
            var empresa = lista.First();
            Assert.Equal(1, empresa.Id);
            Assert.Equal("Ativa 1", empresa.RazaoSocial);

            _empresaRepoMock.Verify(
                r => r.ListAsync(It.IsAny<Expression<Func<Empresa, bool>>>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // =========================================================
        // ObterPorIdAsync - encontrado
        // =========================================================
        [Fact(DisplayName = "[Service-MOQ] ObterPorIdAsync deve retornar DTO quando existir e não estiver excluída")]
        [Trait("Category", "Service.Moq")]
        [Trait("SubCategory", "EmpresaService")]
        public async Task ObterPorIdAsync_Deve_Retornar_Detalhe_Quando_Existir()
        {
            // Arrange
            var entity = new Empresa
            {
                Id = 10,
                RazaoSocial = "ConsertaSmart ME",
                NomeFantasia = "ConsertaSmart",
                Cnpj = "12345678000199",
                FlExcluido = false
            };

            _empresaRepoMock
                .Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            // Act
            var dto = await _service.ObterPorIdAsync(10, CancellationToken.None);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(10, dto!.Id);
            Assert.Equal(entity.RazaoSocial, dto.RazaoSocial);

            _empresaRepoMock.Verify(
                r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // =========================================================
        // ObterPorIdAsync - não encontrado
        // =========================================================
        [Fact(DisplayName = "[Service-MOQ] ObterPorIdAsync deve retornar null quando não existir")]
        [Trait("Category", "Service.Moq")]
        [Trait("SubCategory", "EmpresaService")]
        public async Task ObterPorIdAsync_Deve_Retornar_Null_Quando_Nao_Existir()
        {
            // Arrange
            _empresaRepoMock
                .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Empresa?)null);

            // Act
            var dto = await _service.ObterPorIdAsync(999, CancellationToken.None);

            // Assert
            Assert.Null(dto);

            _empresaRepoMock.Verify(
                r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact(DisplayName = "[Service-MOQ] ObterPorIdAsync deve retornar null quando FlExcluido = true")]
        [Trait("Category", "Service.Moq")]
        [Trait("SubCategory", "EmpresaService")]
        public async Task ObterPorIdAsync_Deve_Retornar_Null_Quando_FlExcluido()
        {
            // Arrange
            var entity = new Empresa
            {
                Id = 20,
                RazaoSocial = "Excluída",
                NomeFantasia = "X",
                Cnpj = "00000000000000",
                FlExcluido = true
            };

            _empresaRepoMock
                .Setup(r => r.GetByIdAsync(20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            // Act
            var dto = await _service.ObterPorIdAsync(20, CancellationToken.None);

            // Assert
            Assert.Null(dto);

            _empresaRepoMock.Verify(
                r => r.GetByIdAsync(20, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // =========================================================
        // SoftDeleteAsync
        // =========================================================
        [Fact(DisplayName = "[Service-MOQ] SoftDeleteAsync deve marcar FlExcluido = true e chamar Update + Commit")]
        [Trait("Category", "Service.Moq")]
        [Trait("SubCategory", "EmpresaService")]
        public async Task SoftDeleteAsync_Deve_Marcar_FlExcluido_Quando_Encontrada()
        {
            // Arrange
            var entity = new Empresa
            {
                Id = 30,
                RazaoSocial = "Empresa Ativa",
                NomeFantasia = "Ativa",
                Cnpj = "12345678000199",
                FlExcluido = false
            };

            _empresaRepoMock
                .Setup(r => r.GetByIdAsync(30, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _empresaRepoMock
                .Setup(r => r.Update(It.IsAny<Empresa>()));

            // Act
            var ok = await _service.SoftDeleteAsync(30, usuarioId: 99, CancellationToken.None);

            // Assert
            Assert.True(ok);
            Assert.True(entity.FlExcluido);
            Assert.Equal(99, entity.AlteradoPor);
            Assert.NotNull(entity.DataAlteracao);

            _empresaRepoMock.Verify(
                r => r.GetByIdAsync(30, It.IsAny<CancellationToken>()),
                Times.Once);

            _empresaRepoMock.Verify(
                r => r.Update(It.IsAny<Empresa>()),
                Times.Once);

            _uowMock.Verify(
                u => u.CommitAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact(DisplayName = "[Service-MOQ] SoftDeleteAsync deve retornar false quando empresa não encontrada ou já excluída")]
        [Trait("Category", "Service.Moq")]
        [Trait("SubCategory", "EmpresaService")]
        public async Task SoftDeleteAsync_Deve_Retornar_False_Quando_Nao_Encontrada_Ou_Ja_Excluida()
        {
            // 1) Não encontrada
            _empresaRepoMock
                .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Empresa?)null);

            var okNotFound = await _service.SoftDeleteAsync(999, usuarioId: 1, CancellationToken.None);
            Assert.False(okNotFound);

            _empresaRepoMock.Verify(
                r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()),
                Times.Once);

            // 2) Já excluída
            var excluida = new Empresa
            {
                Id = 40,
                RazaoSocial = "Excluída",
                FlExcluido = true
            };

            _empresaRepoMock
                .Setup(r => r.GetByIdAsync(40, It.IsAny<CancellationToken>()))
                .ReturnsAsync(excluida);

            var okAlreadyDeleted = await _service.SoftDeleteAsync(40, usuarioId: 1, CancellationToken.None);
            Assert.False(okAlreadyDeleted);

            _empresaRepoMock.Verify(
                r => r.GetByIdAsync(40, It.IsAny<CancellationToken>()),
                Times.Once);

            _empresaRepoMock.Verify(
                r => r.Update(It.IsAny<Empresa>()),
                Times.Never);

            _uowMock.Verify(
                u => u.CommitAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
