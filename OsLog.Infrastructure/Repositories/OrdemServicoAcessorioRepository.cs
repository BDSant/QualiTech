using OsLog.Application.Interfaces.Repositories;
using OsLog.Domain.Entities;
using OsLog.Infrastructure.EntityFramework;

namespace OsLog.Infrastructure.Repositories;

public class OrdemServicoAcessorioRepository : GenericRepository<OrdemServicoAcessorio>, IOrdemServicoAcessorioRepository
{
    public OrdemServicoAcessorioRepository(AppDbContext context) : base(context) { }
}


