using Echo.Domain.Data;

namespace Echo.Infrastructure.Data;

public class UnitOfWork(AppDbContext appDbContext) : IUnitOfWork
{
    private bool _disposed;

    public async Task<int> CommitAsync(CancellationToken ct = default)
    {
        return await appDbContext.SaveChangesAsync(ct);
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
