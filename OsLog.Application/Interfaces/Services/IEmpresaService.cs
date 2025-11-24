using OsLog.Application.DTOs.Empresa;

namespace OsLog.Application.Interfaces.Services;

public interface IEmpresaService
{
    Task<IReadOnlyList<EmpresaListDto>> ListarAsync(CancellationToken ct);
    Task<EmpresaDetailDto?> ObterPorIdAsync(int id, CancellationToken ct);
    Task<int> CriarEmpresaAsync(EmpresaCreateDto dto, int usuarioId, CancellationToken ct);
    Task<bool> SoftDeleteAsync(int id, int usuarioId, CancellationToken ct);
}
