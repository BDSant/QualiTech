using OsLog.Application.Ports.Persistence.Repositories;
using OsLog.Domain.Entities;
using OsLog.Infrastructure.EntityFramework;

namespace OsLog.Infrastructure.Repositories;

public class OrcamentoItemRepository : GenericRepository<OrcamentoItem>, IOrcamentoItemRepository
{
    public OrcamentoItemRepository(AppDbContext context) : base(context) { }
}


