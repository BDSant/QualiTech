using OsLog.Domain.Entities;

namespace OsLog.Application.Interfaces.Repositories;

public interface IUsuarioAcessoRepository : IGenericRepository<UsuarioAcesso>
{
    Task<List<UsuarioAcesso>> ObterAcessosPorUsuarioAsync(string userId, CancellationToken ct = default);
}
