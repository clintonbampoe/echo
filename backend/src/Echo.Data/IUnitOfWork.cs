namespace Echo.Data;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    Task<int> CommitAsync(CancellationToken ct = default);

    /// <summary>
    /// Runs <paramref name="work"/> in one database transaction: it is committed when the work
    /// returns, and rolled back if the work throws. Needed when a bulk statement such as
    /// ExecuteUpdate (which runs at once, not on CommitAsync) must succeed or fail together with
    /// the tracked changes saved by CommitAsync.
    /// </summary>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> work, CancellationToken ct = default);
}
