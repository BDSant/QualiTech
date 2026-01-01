using OsLog.Application.Ports.Persistence.Repositories;
using OsLog.Domain.Entities;
using OsLog.Infrastructure.EntityFramework;

namespace OsLog.Infrastructure.Repositories;

public class PagamentoRepository : GenericRepository<PagamentoOs>, IPagamentoRepository
{
    public PagamentoRepository(AppDbContext context) : base(context) { }

    public Task AtualizarAsync(PagamentoOs pagamento, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<PagamentoOs?> ObterPorIdAsync(int idPagamento, int idOrdemServico, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}