using OsLog.Application.DTOs.OrdemServico;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OsLog.Application.Services
{
    public interface IOrdemServicoService
    {
        Task<int> AbrirOsAsync(OrdemServicoCreateDto dto, int usuarioId, CancellationToken ct);
        Task<List<OrdemServicoListDto>> ListarAsync(CancellationToken ct);
        Task<OrdemServicoDetailDto?> ObterPorIdAsync(int id, CancellationToken ct);
        Task AtualizarStatusAsync(int idOs, string novoStatus, int usuarioId, CancellationToken ct);
        Task ConfirmarPagamentoSinalAsync(int idOs, int usuarioId, CancellationToken ct);
        Task SoftDeleteAsync(int idOs, int usuarioId, CancellationToken ct);
        Task<List<OrcamentoItemDto>> ListarItensAsync(int osId, CancellationToken ct);
        Task AdicionarItemAsync(int osId, string tipoItem, string descricao, decimal quantidade, decimal valorUnitario, int usuarioId, CancellationToken ct);
        Task RemoverItemAsync(int osId, int itemId, int usuarioId, CancellationToken ct);
        Task<List<OrdemServicoAcessorioDto>> ListarAcessoriosAsync(int osId, CancellationToken ct);
        Task AdicionarAcessorioAsync(int osId, string descricao, int usuarioId, CancellationToken ct);
        Task AtualizarDevolucaoAcessorioAsync(int osId, int acessorioId, bool devolvido, int usuarioId, CancellationToken ct);
        Task RemoverAcessorioAsync(int osId, int acessorioId, int usuarioId, CancellationToken ct);
        Task<List<OrdemServicoFotoDto>> ListarFotosAsync(int osId, CancellationToken ct);
        Task AdicionarFotoAsync(int osId, string caminhoArquivo, string? descricao, int usuarioId, CancellationToken ct);
        Task RemoverFotoAsync(int osId, int fotoId, int usuarioId, CancellationToken ct);
    }
}
