using OsLog.Application.Interfaces.Repositories;
using OsLog.Domain.Entities;
using OsLog.Infrastructure.EntityFramework;

namespace OsLog.Infrastructure.Repositories;

public class EmpresaRepository : GenericRepository<Empresa>, IEmpresaRepository
{
    public EmpresaRepository(AppDbContext context) : base(context)
    {
    }
}
