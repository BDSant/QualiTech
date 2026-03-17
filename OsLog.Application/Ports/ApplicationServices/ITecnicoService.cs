using OsLog.Application.DTOs.Tecnicos;
using OsLog.Domain.Entities;

namespace OsLog.Application.Ports.ApplicationServices;

public interface ITecnicoService
{
    Task<int> CreateAsync(TecnicoCreateDto dto, int usuarioId, CancellationToken ct);
    Task<TecnicoDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<TecnicoDto>> ListAsync(CancellationToken ct);
    Task InativarAsync(Guid id, int usuarioId, CancellationToken ct);
}
