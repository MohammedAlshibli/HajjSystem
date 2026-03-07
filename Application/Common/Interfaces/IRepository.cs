using System.Linq.Expressions;

namespace HajjSystem.Application.Common.Interfaces;

/// <summary>
/// Generic repository — Application layer has zero EF Core dependency.
/// All async execution methods live here.
/// </summary>
public interface IRepository<T> where T : class
{
    IQueryable<T> Query();

    Task<T?>       GetByIdAsync(int id);
    Task<List<T>>  ToListAsync(IQueryable<T> query);
    Task<T?>       FirstOrDefaultAsync(IQueryable<T> query);
    Task<bool>     AnyAsync(IQueryable<T> query);
    Task<int>      CountAsync(IQueryable<T> query);

    void Add(T entity);
    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}
