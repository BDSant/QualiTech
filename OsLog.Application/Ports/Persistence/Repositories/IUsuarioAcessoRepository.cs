using OsLog.Domain.Entities;

namespace OsLog.Application.Ports.Persistence.Repositories;

public interface IUsuarioAcessoRepository : IGenericRepository<UsuarioAcesso>
{
    Task<UsuarioAcesso?> ObterPorUserIdAsync(string userId, CancellationToken ct = default);

    Task<IReadOnlyCollection<UsuarioAcesso>> ObterListaPorUserIdAsync(string userId, CancellationToken ct = default);
}