namespace Echo.Data;

public class UnitOfWork(AppDbContext appDbContext) : IUnitOfWork
{
    private bool _disposed;

    public async Task<int> CommitAsync(CancellationToken ct = default)
    {
        return await appDbContext.SaveChangesAsync(ct);
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> work,
        CancellationToken ct = default
    )
    {
        // Disposing the transaction without committing rolls it back.
        await using var transaction = await appDbContext.Database.BeginTransactionAsync(ct);
        await work(ct);
        await transaction.CommitAsync(ct);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            appDbContext.Dispose();
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await appDbContext.DisposeAsync();
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }
}
