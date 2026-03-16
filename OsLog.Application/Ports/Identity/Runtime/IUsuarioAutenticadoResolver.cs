using OsLog.Domain.Entities;

namespace OsLog.Application.Ports.Identity.Runtime;

public interface IUsuarioAutenticadoResolver
{
    Task<int?> ObterUsuarioIdAsync(string userId, CancellationToken ct = default);
}