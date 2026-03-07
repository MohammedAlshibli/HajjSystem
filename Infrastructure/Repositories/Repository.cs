using HajjSystem.Application.Common.Interfaces;
using HajjSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HajjSystem.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _ctx;
    protected readonly DbSet<T>     _set;

    public Repository(AppDbContext ctx)
    {
        _ctx = ctx;
        _set = ctx.Set<T>();
    }

    public IQueryable<T> Query() => _set.AsQueryable();

    public Task<T?>        GetByIdAsync(int id)              => _set.FindAsync(id).AsTask()!;
    public Task<List<T>>   ToListAsync(IQueryable<T> query)  => query.ToListAsync();
    public Task<T?>        FirstOrDefaultAsync(IQueryable<T> query) => query.FirstOrDefaultAsync();
    public Task<bool>      AnyAsync(IQueryable<T> query)     => query.AnyAsync();
    public Task<int>       CountAsync(IQueryable<T> query)   => query.CountAsync();

    public void Add(T entity)                        => _set.Add(entity);
    public void Update(T entity)                     => _ctx.Entry(entity).State = EntityState.Modified;
    public void Remove(T entity)                     => _set.Remove(entity);
    public void RemoveRange(IEnumerable<T> entities) => _set.RemoveRange(entities);
}
