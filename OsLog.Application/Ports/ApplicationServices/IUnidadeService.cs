using OsLog.Application.DTOs.Empresa;

namespace OsLog.Application.Ports.ApplicationServices;

public interface IUnidadeService
{
    Task<int> Create(int empresaId, UnidadeCreateDto dto, int usuarioId, CancellationToken ct);
    Task<IReadOnlyList<UnidadeDto>> GetAll(CancellationToken ct);
    Task<IReadOnlyList<UnidadeDto>> GetById(int empresaId, CancellationToken ct);
    Task<bool> Delete(int unidadeId, int usuarioId, CancellationToken ct);
}
