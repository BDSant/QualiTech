using Microsoft.EntityFrameworkCore;
using OsLog.Application.Ports.Persistence.Repositories;
using OsLog.Domain.Entities;
using OsLog.Infrastructure.EntityFramework;

namespace OsLog.Infrastructure.Repositories;

public class UsuarioAcessoRepository : GenericRepository<UsuarioAcesso>, IUsuarioAcessoRepository
{
    public UsuarioAcessoRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<UsuarioAcesso?> ObterPorUserIdAsync(string userId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UsuarioId == userId && !x.Ativo, ct);
    }

    public async Task<IReadOnlyCollection<UsuarioAcesso>> ObterListaPorUserIdAsync(string userId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(x => x.Empresa)
            .Include(x => x.Unidade)
            .Where(x => x.UsuarioId == userId && !x.Ativo)
            .ToListAsync(ct);
    }
}