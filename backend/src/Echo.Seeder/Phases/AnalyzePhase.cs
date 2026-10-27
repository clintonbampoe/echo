using Echo.Seeder.Core;
using Npgsql;

namespace Echo.Seeder.Phases;

/// <summary>
/// Brings the planner's statistics up to date, and makes sure the query
/// statistics extension exists.
/// <br/><br/>
/// Every other phase writes rows with COPY. Postgres does not update planner
/// statistics for a bulk load — it samples them lazily, from the table's
/// pages, and a table that was just written wholesale has never been
/// sampled. The planner is therefore choosing plans from the estimates of an
/// empty table, and the baseline measures plans Postgres would never choose
/// on a database that had been running for years.
/// <br/><br/>
/// ANALYZE samples the tables and writes the result into pg_statistic. It is
/// the difference between measuring the queries and measuring the queries
/// with the wrong plans.
/// <br/><br/>
/// It also creates pg_stat_statements if it is missing: that extension is
/// the ranked list of the slowest queries the server has run, and it is the
/// instrument every later move reads.
/// </summary>
public sealed class AnalyzePhase : ISeedPhase
{
    public string Name => "Analyze";

    public async Task RunAsync(SeederContext ctx, CancellationToken ct)
    {
        await Execute(ctx, "CREATE EXTENSION IF NOT EXISTS pg_stat_statements", ct);
        await Execute(ctx, "ANALYZE", ct);

        await using var cmd = new NpgsqlCommand(
            """
            SELECT relname, n_live_tup
            FROM pg_stat_user_tables
            WHERE n_live_tup > 0
            ORDER BY n_live_tup DESC
            LIMIT 5
            """,
            ctx.Connection
        );
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var rows = new List<string>();
        var total = 0L;
        var count = 0;
        while (await reader.ReadAsync(ct))
        {
            var name = reader.GetString(0);
            var live = reader.GetInt64(1);
            total += live;
            count++;
            rows.Add($"{name} ({live:N0})");
        }

        ctx.Logger.AnalyzeCompleted(count, total, string.Join(", ", rows));
    }

    private static async Task Execute(SeederContext ctx, string sql, CancellationToken ct)
    {
        await using var cmd = new NpgsqlCommand(sql, ctx.Connection);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
