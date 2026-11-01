using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Echo.IntegrationTests.Support;

/// <summary>Counts every command EF executes, so tests can pin how many queries a flow costs.</summary>
public sealed class CommandCounter : DbCommandInterceptor
{
    private readonly List<string> _statements = [];
    private int _count;

    public int Count => _count;

    /// <summary>The SQL of every command executed since the last reset.</summary>
    public IReadOnlyList<string> Statements
    {
        get
        {
            lock (_statements)
                return [.. _statements];
        }
    }

    public void Reset()
    {
        Interlocked.Exchange(ref _count, 0);
        lock (_statements)
            _statements.Clear();
    }

    private void Record(DbCommand command)
    {
        Interlocked.Increment(ref _count);
        lock (_statements)
            _statements.Add(command.CommandText);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken ct = default
    )
    {
        Record(command);
        return base.ReaderExecutingAsync(command, eventData, result, ct);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default
    )
    {
        Record(command);
        return base.NonQueryExecutingAsync(command, eventData, result, ct);
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken ct = default
    )
    {
        Record(command);
        return base.ScalarExecutingAsync(command, eventData, result, ct);
    }
}
