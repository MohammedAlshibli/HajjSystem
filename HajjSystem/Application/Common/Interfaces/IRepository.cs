using System.Linq.Expressions;

namespace HajjSystem.Application.Common.Interfaces;

/// <summary>
/// Generic repository — standard CRUD + IQueryable.
/// No ITrackable, no external Core library needed.
/// </summary>
public interface IRepository<T> where T : class
{
    IQueryable<T> Query();
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    void Add(T entity);
    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}
