using OsLog.Application.Ports.Persistence.Repositories;
using OsLog.Domain.Entities;

namespace OsLog.Domain.Interfaces.Repositories;

public interface IUsuarioAcessoRepository : IGenericRepository<UsuarioAcesso>
{
    Task<UsuarioAcesso?> ObterPorUserIdAsync(string userId, CancellationToken ct = default);
    Task<IReadOnlyList<UsuarioAcesso>> ObterListaPorUserIdAsync(string userId, CancellationToken ct = default);
}