using OsLog.Application.DTOs.Empresa;

namespace OsLog.Application.Ports.ApplicationServices;

public interface IEmpresaService
{
    Task<IReadOnlyList<EmpresaListDto>> GetAll(CancellationToken ct);
    Task<EmpresaDetailDto?> GetById(Guid id, CancellationToken ct);
    Task<Guid> Create(EmpresaCreateDto dto, int usuarioId, CancellationToken ct);
    Task<bool> Delete(Guid id, int usuarioId, CancellationToken ct);
}
