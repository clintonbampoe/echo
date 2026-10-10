using Npgsql;

namespace Echo.Seeder.Core;

/// <summary>
/// The single write path for all volume data.
/// <br/><br/>
///
/// Takes a lazy sequence, breaks it into batches of Options.BatchSize, and
/// hands each batch to a COPY writer. Each batch runs inside its own
/// transaction — if a batch fails, only that batch is rolled back and the
/// exception propagates so the seeder can stop cleanly.
///
/// </summary>
public static class CopyBatch
{
    /// <summary>
    /// Writes all items from <paramref name="source"/> in batches.
    ///
    /// <paramref name="copyBatch"/> receives each batch and the open
    /// connection. It must open a binary import and complete it — the
    /// transaction wrapping the batch is handled here.
    ///
    /// <paramref name="onProgress"/> is called after each committed batch
    /// with the cumulative row count. Pass null if you don't need it.
    /// </summary>
    public static async Task WriteAsync<T>(
        NpgsqlConnection conn,
        IEnumerable<T> source,
        int batchSize,
        Func<NpgsqlConnection, IReadOnlyList<T>, CancellationToken, Task> copyBatch,
        Action<int>? onProgress,
        CancellationToken ct
    )
    {
        var batch = new List<T>(batchSize);
        var total = 0;

        foreach (var item in source)
        {
            ct.ThrowIfCancellationRequested();
            batch.Add(item);

            if (batch.Count < batchSize)
                continue;

            await CommitBatchAsync(conn, batch, copyBatch, ct);
            total += batch.Count;
            onProgress?.Invoke(total);
            batch.Clear();
        }

        if (batch.Count > 0)
        {
            await CommitBatchAsync(conn, batch, copyBatch, ct);
            total += batch.Count;
            onProgress?.Invoke(total);
        }
    }

    private static async Task CommitBatchAsync<T>(
        NpgsqlConnection conn,
        IReadOnlyList<T> batch,
        Func<NpgsqlConnection, IReadOnlyList<T>, CancellationToken, Task> copyBatch,
        CancellationToken ct
    )
    {
        await using var tx = await conn.BeginTransactionAsync(ct);
        try
        {
            await copyBatch(conn, batch, ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}
