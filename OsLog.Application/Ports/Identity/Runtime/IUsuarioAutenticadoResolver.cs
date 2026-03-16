using OsLog.Domain.Entities;

namespace OsLog.Application.Ports.Users;

public interface IUsuarioAutenticadoResolver
{
    Task<int?> ObterPorUserIdAsync(string userId, CancellationToken ct = default);
}