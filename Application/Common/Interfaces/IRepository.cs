using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace HajjSystem.Application.Common.Interfaces;

public interface IRepository<T> where T : class
{
    IQueryable<T> Query();

    Task<T?>       GetByIdAsync(int id);
    Task<List<T>>  ToListAsync(IQueryable<T> query);
    Task<T?>       FirstOrDefaultAsync(IQueryable<T> query);
    Task<bool>     AnyAsync(IQueryable<T> query);
    Task<int>      CountAsync(IQueryable<T> query);
    Task<List<T>>  FindAsync(Expression<Func<T, bool>> predicate);

    void Add(T entity);
    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}
