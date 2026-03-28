using OsLog.Application.DTOs.Empresa;

namespace OsLog.Application.Ports.ApplicationServices;

public interface IEmpresaService
{
    Task<Guid> Create(EmpresaCreateDto dto, string usuarioId, CancellationToken ct);
    Task<IReadOnlyList<EmpresaListDto>> GetAll(string usuarioId, CancellationToken ct);
    Task<EmpresaDetailDto?> GetById(Guid id, string usuarioId, CancellationToken ct);
    Task<bool> Delete(Guid id, string usuarioId, CancellationToken ct);
}
