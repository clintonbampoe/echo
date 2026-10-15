using Echo.Data;
using Npgsql;

namespace Echo.Seeder.Core;

/// <summary>
/// Everything a phase needs to do its work.
///
/// Passed into every ISeedPhase.RunAsync call. Phases read from it and write
/// their output back into the congregation's CongregationSeed entry in State.
/// </summary>
public sealed class SeederContext(
    AppDbContext db,
    NpgsqlConnection connection,
    SeederOptions options,
    TimeProvider timeProvider,
    ILogger logger
)
{
    public AppDbContext Db { get; } = db;
    public NpgsqlConnection Connection { get; } = connection;
    public SeederOptions Options { get; } = options;
    public DateTime Now { get; } = timeProvider.GetUtcNow().UtcDateTime;
    public DateOnly Today => DateOnly.FromDateTime(Now);
    public List<CongregationSeed> State { get; } = [];
    public ILogger Logger { get; } = logger;

    /// <summary>
    /// Creates a fresh Rng for a named phase, optionally narrowed to a scope
    /// (usually a congregation name). Same global seed + same phase + same
    /// scope always produces the same sequence, independent of everything else.
    /// </summary>
    public Rng RngFor(string phaseName, string? scope = null) =>
        Rng.ForPhase(Options.Seed, phaseName, scope);
}
