using OsLog.Application.Ports.Persistence.Repositories;
using OsLog.Domain.Entities;
using OsLog.Infrastructure.EntityFramework;

namespace OsLog.Infrastructure.Repositories;

public class TecnicoRepository : GenericRepository<Tecnico>, ITecnicoRepository
{
    public TecnicoRepository(AppDbContext context) : base(context) { }
}

