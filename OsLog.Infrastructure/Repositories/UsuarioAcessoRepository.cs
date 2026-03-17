using Microsoft.EntityFrameworkCore;
using OsLog.Domain.Entities;
using OsLog.Domain.Interfaces.Repositories;
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
            .FirstOrDefaultAsync(x => x.UsuarioId == userId && x.Ativo, ct);
    }

    public async Task<IReadOnlyList<UsuarioAcesso>> ObterListaPorUserIdAsync(string userId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(x => x.UsuarioId == userId && x.Ativo)
            .ToListAsync(ct);
    }
}