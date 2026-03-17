using OsLog.Application.DTOs.Unidade;

namespace OsLog.Application.Ports.ApplicationServices;

public interface IUnidadeService
{
    Task<int> Create(Guid empresaId, UnidadeCreateDto dto, int usuarioId, CancellationToken ct);
    Task<IReadOnlyList<UnidadeDto>> GetAll(CancellationToken ct);
    Task<IReadOnlyList<UnidadeDto>> GetById(Guid empresaId, CancellationToken ct);
    Task<bool> Delete(Guid empresaId, int unidadeId, int usuarioId, CancellationToken ct);
}
