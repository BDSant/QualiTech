using OsLog.Application.DTOs.Empresa;

namespace OsLog.Application.Interfaces.Services;

public interface IUsuarioAcessoService
{
    Task<IReadOnlyCollection<EmpresaAcessoDto>> ObterAcessoPorUsuarioAsync(string userId, CancellationToken ct = default);
}
