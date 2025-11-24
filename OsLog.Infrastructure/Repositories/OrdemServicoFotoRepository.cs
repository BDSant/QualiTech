using OsLog.Application.Interfaces.Repositories;
using OsLog.Domain.Entities;
using OsLog.Infrastructure.EntityFramework;

namespace OsLog.Infrastructure.Repositories;

public class OrdemServicoFotoRepository : GenericRepository<OrdemServicoFoto>, IOrdemServicoFotoRepository
{
    public OrdemServicoFotoRepository(AppDbContext context) : base(context) { }
}


