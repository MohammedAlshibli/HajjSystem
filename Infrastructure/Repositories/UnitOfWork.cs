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

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        var tx = await _ctx.Database.BeginTransactionAsync(ct);
        return new EfTransaction(tx);
    }

    public void Dispose() => _ctx.Dispose();
}

/// <summary>Wraps IDbContextTransaction behind the clean ITransaction interface.</summary>
internal sealed class EfTransaction : ITransaction
{
    private readonly IDbContextTransaction _tx;
    public EfTransaction(IDbContextTransaction tx) => _tx = tx;

    public Task CommitAsync(CancellationToken ct = default)  => _tx.CommitAsync(ct);
    public Task RollbackAsync(CancellationToken ct = default) => _tx.RollbackAsync(ct);

    public void Dispose()          => _tx.Dispose();
    public ValueTask DisposeAsync() => _tx.DisposeAsync();
}
