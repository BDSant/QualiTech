using OsLog.Application.DTOs.Empresa;

namespace OsLog.Application.Ports.ApplicationServices;

public interface IEmpresaService
{
    Task<IReadOnlyList<EmpresaListDto>> GetAll(CancellationToken ct);
    Task<EmpresaDetailDto?> GetById(int id, CancellationToken ct);
    Task<int> Create(EmpresaCreateDto dto, int usuarioId, CancellationToken ct);
    Task<bool> Delete(int id, int usuarioId, CancellationToken ct);
}
