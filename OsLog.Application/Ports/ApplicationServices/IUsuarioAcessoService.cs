using OsLog.Application.DTOs.Empresa;

namespace OsLog.Application.Ports.ApplicationServices;

public interface IUsuarioAcessoService
{
    Task<IReadOnlyCollection<EmpresaAcessoDto>> ObterAcessoPorUsuarioAsync(string userId, CancellationToken ct = default);
}
