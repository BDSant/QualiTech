using OsLog.Application.Ports.Persistence.Repositories;
using OsLog.Domain.Entities;
using OsLog.Infrastructure.EntityFramework;
using System.Linq.Expressions;

namespace OsLog.Infrastructure.Repositories;

public class EmpresaRepository : GenericRepository<Empresa>, IEmpresaRepository
{
    public EmpresaRepository(AppDbContext context) : base(context)
    {
    }
}
