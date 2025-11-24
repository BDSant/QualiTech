using OsLog.Application.Interfaces.Repositories;
using OsLog.Domain.Entities;
using OsLog.Infrastructure.EntityFramework;

namespace OsLog.Infrastructure.Repositories;

public class OrdemServicoComissaoRepository : GenericRepository<OrdemServicoComissao>, IOrdemServicoComissaoRepository
{
    public OrdemServicoComissaoRepository(AppDbContext context) : base(context) { }
}


