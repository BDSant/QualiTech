using OsLog.Application.DTOs.Unidade;

namespace OsLog.Application.Ports.ApplicationServices;

public interface IUnidadeService
{
    Task<int> Create(Guid empresaId, UnidadeCreateDto dto, CancellationToken ct);
    Task<IReadOnlyList<UnidadeDto>> GetAllByEmpresa(Guid empresaId, CancellationToken ct);
    Task<UnidadeDto?> GetById(int id, Guid empresaId, CancellationToken ct);
    Task<bool> Delete(int id, Guid empresaId, CancellationToken ct);
}