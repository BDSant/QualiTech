using OsLog.Application.DTOs.Tecnicos;
using OsLog.Domain.Entities;

namespace OsLog.Application.Interfaces.Services;

public interface ITecnicoService
{
    Task<int> CreateAsync(TecnicoCreateDto dto, int usuarioId, CancellationToken ct);
    Task<TecnicoDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<List<TecnicoDto>> ListAsync(CancellationToken ct);
    Task InativarAsync(int id, int usuarioId, CancellationToken ct);
}
