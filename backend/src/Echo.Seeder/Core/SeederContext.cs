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
    public List<CongregationSeed> State { get; } = [];
    public ILogger Logger { get; } = logger;

    /// <summary>
    /// Creates a fresh Rng for a named phase. Same global seed + same phase
    /// name always produces the same sequence, independent of other phases.
    /// </summary>
    public Rng RngFor(string phaseName) => Rng.ForPhase(Options.Seed, phaseName);
}
