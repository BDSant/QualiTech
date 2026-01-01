using OsLog.Domain.Entities;

namespace OsLog.Application.Ports.Persistence.Repositories;

public interface IUsuarioAcessoRepository : IGenericRepository<UsuarioAcesso>
{
    Task<List<UsuarioAcesso>> ObterAcessosPorUsuarioAsync(string userId, CancellationToken ct = default);
}
