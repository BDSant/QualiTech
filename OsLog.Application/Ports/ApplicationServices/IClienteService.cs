using OsLog.Application.DTOs.Clientes;

namespace OsLog.Application.Ports.ApplicationServices
{
    public interface IClienteService
    {
        Task<IReadOnlyList<ClienteDto>> ListarAsync(CancellationToken ct);  
        Task<ClienteDto?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<int> CreateAsync(ClienteCreateDto dto, int usuarioId, CancellationToken ct);
        Task SoftDeleteAsync(Guid id, int usuarioId, CancellationToken ct);
    }
}
