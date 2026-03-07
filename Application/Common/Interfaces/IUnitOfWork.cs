namespace HajjSystem.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default);
}

/// <summary>
/// Clean abstraction over IDbContextTransaction.
/// Application layer knows nothing about EF Core.
/// </summary>
public interface ITransaction : IDisposable, IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}
