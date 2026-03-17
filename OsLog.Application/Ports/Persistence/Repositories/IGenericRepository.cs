using System.Linq.Expressions;

namespace OsLog.Application.Ports.Persistence.Repositories;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetById(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAll(CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetById(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Delete(T entity);
}
