using OsLog.Infrastructure.EntityFramework;
using OsLog.Domain.Entities;
using OsLog.Application.Ports.Persistence.Repositories;

namespace OsLog.Infrastructure.Repositories;

public class ClienteRepository : GenericRepository<Cliente>, IClienteRepository
{
    public ClienteRepository(AppDbContext context) : base(context) { }
}


