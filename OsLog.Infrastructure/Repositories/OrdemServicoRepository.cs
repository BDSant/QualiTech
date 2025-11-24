using Microsoft.EntityFrameworkCore;
using OsLog.Application.Interfaces.Repositories;
using OsLog.Domain.Entities;
using OsLog.Infrastructure.EntityFramework;

namespace OsLog.Infrastructure.Repositories;

public partial class OrdemServicoRepository : GenericRepository<OrdemServico>, IOrdemServicoRepository
{
    public OrdemServicoRepository(AppDbContext context) : base(context) { }

    public async Task<OrdemServico?> GetDetalhadaAsync(int id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(o => o.Cliente)
            .Include(o => o.Tecnico)
            .Include(o => o.Itens)
            .Include(o => o.Acessorios)
            .Include(o => o.Fotos)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

}
