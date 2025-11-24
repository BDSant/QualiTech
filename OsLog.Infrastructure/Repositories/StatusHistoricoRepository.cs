using OsLog.Application.Interfaces.Repositories;
using OsLog.Domain.Entities;
using OsLog.Infrastructure.EntityFramework;

namespace OsLog.Infrastructure.Repositories;

public class StatusHistoricoRepository : GenericRepository<StatusHistorico>, IStatusHistoricoRepository
{
    public StatusHistoricoRepository(AppDbContext context) : base(context) { }
}
