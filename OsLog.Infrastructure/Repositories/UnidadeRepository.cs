using OsLog.Application.Ports.Persistence.Repositories;
using OsLog.Domain.Entities;
using OsLog.Infrastructure.EntityFramework;

namespace OsLog.Infrastructure.Repositories;

public class UnidadeRepository : GenericRepository<Unidade>, IUnidadeRepository
{
    public UnidadeRepository(AppDbContext context) : base(context)
    {
    }
}
