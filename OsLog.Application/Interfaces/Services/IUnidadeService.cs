using OsLog.Application.DTOs.Empresa;

namespace OsLog.Application.Interfaces.Services;

public interface IUnidadeService
{
    Task<int> CriarUnidadeAsync(int empresaId, UnidadeCreateDto dto, int usuarioId, CancellationToken ct);
    Task<IReadOnlyList<UnidadeDto>> ListarTodasAsync(CancellationToken ct);
    Task<IReadOnlyList<UnidadeDto>> ListarPorEmpresaAsync(int empresaId, CancellationToken ct);
    Task<bool> SoftDeleteAsync(int id, int usuarioId, CancellationToken ct);
}
