using HajjSystem.Application.Common.Interfaces;
using HajjSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace HajjSystem.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _ctx;
    public UnitOfWork(AppDbContext ctx) => _ctx = ctx;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _ctx.SaveChangesAsync(ct);

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
        => _ctx.Database.BeginTransactionAsync(ct);

    public void Dispose() => _ctx.Dispose();
}
