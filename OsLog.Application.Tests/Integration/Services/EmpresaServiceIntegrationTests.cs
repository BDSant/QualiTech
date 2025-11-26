using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OsLog.Application.Common;
using OsLog.Application.DTOs.Empresa;
using OsLog.Application.Interfaces.Repositories;
using OsLog.Application.Mapping;
using OsLog.Application.Services;
using OsLog.Domain.Entities;
using OsLog.Infrastructure.EntityFramework;
using OsLog.Infrastructure.Repositories;
using System;

namespace OsLog.Tests.Integration.Services
{
    public class EmpresaServiceIntegrationTests
    {

        /// <summary>
        /// UnitOfWork de teste, minimalista, que só implementa
        /// o que o EmpresaService realmente usa:
        /// - Empresas
        /// - CommitAsync
        /// - DisposeAsync
        /// Os demais repositórios levantam NotImplementedException.
        /// </summary>
        private sealed class TestUnitOfWork : IUnitOfWork
        {
            private readonly AppDbContext _ctx;

            public TestUnitOfWork(AppDbContext ctx)
            {
                _ctx = ctx;
                Empresas = new EmpresaRepository(ctx);
                Unidades = new UnidadeRepository(ctx);
            }

            public IOrdemServicoRepository OrdensServico => throw new NotImplementedException();
            public IOrcamentoItemRepository OrcamentoItens => throw new NotImplementedException();
            public IPagamentoRepository Pagamentos => throw new NotImplementedException();
            public IStatusHistoricoRepository StatusHistoricos => throw new NotImplementedException();
            public IOrdemServicoAcessorioRepository Acessorios => throw new NotImplementedException();
            public IOrdemServicoFotoRepository Fotos => throw new NotImplementedException();
            public IOrdemServicoComissaoRepository Comissoes => throw new NotImplementedException();
            public IClienteRepository Clientes => throw new NotImplementedException();
            public ITecnicoRepository Tecnicos => throw new NotImplementedException();
            public IEmpresaRepository Empresas { get; }
            public IUnidadeRepository Unidades { get; }
            public Task<int> CommitAsync(CancellationToken ct = default) => _ctx.SaveChangesAsync(ct);
            public ValueTask DisposeAsync() => _ctx.DisposeAsync();
        }

        /// <summary>
        /// Cria o AppDbContext in-memory, o repositorio real e o EmpresaService com TestUnitOfWork.
        /// </summary>
        private static (EmpresaService service, AppDbContext ctx) CreateService()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"OsLog_EmpresaService_{Guid.NewGuid()}")
                .Options;

            var ctx = new AppDbContext(options);

            // UnitOfWork de teste (apenas Empresas + CommitAsync)
            var uow = new TestUnitOfWork(ctx);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(typeof(EmpresaProfile).Assembly);
            });

            var mapper = mapperConfig.CreateMapper();
            var service = new EmpresaService(uow, mapper);

            return (service, ctx);
        }

        // ======================================================
        // CriarEmpresaAsync
        // ======================================================
        [Fact(DisplayName = "[Service-INT] CriarEmpresaAsync deve persistir no banco InMemory")]
        [Trait("Category", "Service.Integration")]
        [Trait("SubCategory", "EmpresaService")]
        public async Task CriarEmpresaAsync_Deve_Persistir_No_Banco()
        {
            var (service, ctx) = CreateService();

            var dto = new EmpresaCreateDto
            {
                RazaoSocial = "ConsertaSmart ME",
                NomeFantasia = "ConsertaSmart Centro",
                Cnpj = "12345678000199"
            };

            var id = await service.CriarEmpresaAsync(dto, usuarioId: 1, CancellationToken.None);

            Assert.True(id > 0);

            var entity = await ctx.Empresas.FindAsync(id);
            Assert.NotNull(entity);
            Assert.Equal(dto.RazaoSocial, entity!.RazaoSocial);
            Assert.False(entity.FlExcluido);
        }

        // ======================================================
        // ListarAsync
        // ======================================================
        [Fact(DisplayName = "[Service-INT] ListarAsync deve retornar apenas empresas ativas")]
        [Trait("Category", "Service.Integration")]
        [Trait("SubCategory", "EmpresaService")]
        public async Task ListarAsync_Deve_Retornar_Apenas_Ativas()
        {
            var (service, ctx) = CreateService();

            ctx.Empresas.AddRange(
                new Empresa
                {
                    RazaoSocial = "Ativa 1",
                    NomeFantasia = "Ativa 1",
                    Cnpj = "11111111111111",
                    FlExcluido = false
                },
                new Empresa
                {
                    RazaoSocial = "Excluída",
                    NomeFantasia = "Excluída",
                    Cnpj = "22222222222222",
                    FlExcluido = true
                });

            await ctx.SaveChangesAsync();

            var lista = await service.ListarAsync(CancellationToken.None);

            Assert.Single(lista);
            Assert.Equal("Ativa 1", lista.First().RazaoSocial);
        }

        // ======================================================
        // ObterPorIdAsync
        // ======================================================
        [Fact(DisplayName = "[Service-INT] ObterPorIdAsync deve retornar detalhe quando existir")]
        [Trait("Category", "Service.Integration")]
        [Trait("SubCategory", "EmpresaService")]
        public async Task ObterPorIdAsync_Deve_Retornar_Detalhe_Quando_Existir()
        {
            var (service, ctx) = CreateService();

            var entity = new Empresa
            {
                RazaoSocial = "ConsertaSmart ME",
                NomeFantasia = "ConsertaSmart",
                Cnpj = "12345678000199",
                FlExcluido = false
            };

            ctx.Empresas.Add(entity);
            await ctx.SaveChangesAsync();

            var dto = await service.ObterPorIdAsync(entity.Id, CancellationToken.None);

            Assert.NotNull(dto);
            Assert.Equal(entity.Id, dto!.Id);
            Assert.Equal(entity.RazaoSocial, dto.RazaoSocial);
        }

        [Fact(DisplayName = "[Service-INT] ObterPorIdAsync deve retornar null quando não existir")]
        [Trait("Category", "Service.Integration")]
        [Trait("SubCategory", "EmpresaService")]
        public async Task ObterPorIdAsync_Deve_Retornar_Null_Quando_Nao_Existir()
        {
            var (service, _) = CreateService();

            var dto = await service.ObterPorIdAsync(999, CancellationToken.None);

            Assert.Null(dto);
        }

        // ======================================================
        // SoftDeleteAsync
        // ======================================================
        [Fact(DisplayName = "[Service-INT] SoftDeleteAsync deve marcar FlExcluido = true")]
        [Trait("Category", "Service.Integration")]
        [Trait("SubCategory", "EmpresaService")]
        public async Task SoftDeleteAsync_Deve_Marcar_FlExcluido()
        {
            var (service, ctx) = CreateService();

            var entity = new Empresa
            {
                RazaoSocial = "Empresa Ativa",
                NomeFantasia = "Ativa",
                Cnpj = "12345678000199",
                FlExcluido = false
            };

            ctx.Empresas.Add(entity);
            await ctx.SaveChangesAsync();

            var ok = await service.SoftDeleteAsync(entity.Id, usuarioId: 10, CancellationToken.None);

            Assert.True(ok);

            var reloaded = await ctx.Empresas.FindAsync(entity.Id);
            Assert.NotNull(reloaded);
            Assert.True(reloaded!.FlExcluido);
            Assert.Equal(10, reloaded.AlteradoPor);
        }

        [Fact(DisplayName = "[Service-INT] SoftDeleteAsync deve retornar false quando não existir")]
        [Trait("Category", "Service.Integration")]
        [Trait("SubCategory", "EmpresaService")]
        public async Task SoftDeleteAsync_Deve_Retornar_False_Quando_Nao_Existir()
        {
            var (service, _) = CreateService();

            var ok = await service.SoftDeleteAsync(999, usuarioId: 1, CancellationToken.None);

            Assert.False(ok);
        }
    }
}
