using Microsoft.EntityFrameworkCore;
using OsLog.Application.Interfaces.Repositories;
using OsLog.Domain.Entities;
using OsLog.Infrastructure.EntityFramework;

namespace OsLog.Infrastructure.Repositories;

public class UsuarioAcessoRepository : GenericRepository<UsuarioAcesso>, IUsuarioAcessoRepository
{
    public UsuarioAcessoRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<UsuarioAcesso>> ObterAcessosPorUsuarioAsync(
        string userId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(x => x.Empresa)
            .Include(x => x.Unidade)
            .Where(x => x.UserId == userId && !x.FlExcluido)
            .ToListAsync(ct);
    }
}
