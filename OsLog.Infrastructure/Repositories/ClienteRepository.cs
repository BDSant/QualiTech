using OsLog.Infrastructure.EntityFramework;
using OsLog.Application.Interfaces.Repositories;
using OsLog.Domain.Entities;

namespace OsLog.Infrastructure.Repositories;

public class ClienteRepository : GenericRepository<Cliente>, IClienteRepository
{
    public ClienteRepository(AppDbContext context) : base(context) { }
}


