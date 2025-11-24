using OsLog.Domain.Entities;

namespace OsLog.Application.Interfaces.Repositories;

public interface IOrdemServicoRepository : IGenericRepository<OrdemServico>
{
    Task<OrdemServico?> GetDetalhadaAsync(int id, CancellationToken ct = default);
}
