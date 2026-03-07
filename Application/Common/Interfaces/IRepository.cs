using System.Linq.Expressions;

namespace HajjSystem.Application.Common.Interfaces;

/// <summary>
/// Generic repository — no EF Core dependency in Application layer.
/// All async query methods live here so services never need using Microsoft.EntityFrameworkCore.
/// </summary>
public interface IRepository<T> where T : class
{
    IQueryable<T> Query();

    Task<T?>              GetByIdAsync(int id);
    Task<List<T>>         ToListAsync(IQueryable<T> query);
    Task<T?>              FirstOrDefaultAsync(IQueryable<T> query);
    Task<bool>            AnyAsync(IQueryable<T> query);
    Task<int>             CountAsync(IQueryable<T> query);

    void Add(T entity);
    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}
