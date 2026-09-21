namespace Echo.Data;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    Task<int> CommitAsync(CancellationToken ct = default);
}
