using OsLog.Application.Ports.Persistence.Repositories;

namespace OsLog.Application.Common;

public interface IUnitOfWork : IAsyncDisposable
{
    IPagamentoRepository Pagamentos { get; }
    IStatusHistoricoRepository StatusHistoricos { get; }
    IClienteRepository Clientes { get; }
    ITecnicoRepository Tecnicos { get; }
    IEmpresaRepository Empresas { get; }
    IUnidadeRepository Unidades { get; }
    IUsuarioAcessoRepository UsuarioAcessos { get; }

    Task BeginTransactionAsync(CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}

