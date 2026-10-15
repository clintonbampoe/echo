namespace Echo.Seeder.Core;

/// <summary>
/// The seeder's fixed log messages, in one place. Messages that carry a
/// number or two and are used once (progress lines, per-congregation
/// summaries) are written inline in the phase that logs them.
/// </summary>
internal static partial class SeederLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Seeder starting — Seed={Seed} Reset={Reset} BatchSize={Batch} ReferenceNow={Now:o}"
    )]
    public static partial void Starting(
        this ILogger logger,
        int seed,
        bool reset,
        int batch,
        DateTimeOffset now
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Reset requested — truncating all seeded tables."
    )]
    public static partial void Resetting(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "── Phase: {Phase} ──")]
    public static partial void PhaseStarted(this ILogger logger, string phase);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "── Phase: {Phase} done in {Elapsed:F1}s ──"
    )]
    public static partial void PhaseDone(this ILogger logger, string phase, double elapsed);

    [LoggerMessage(Level = LogLevel.Information, Message = "Seeder complete in {Elapsed:F1}s.")]
    public static partial void Complete(this ILogger logger, double elapsed);

    [LoggerMessage(Level = LogLevel.Error, Message = "Seeder failed after {Elapsed:F1}s.")]
    public static partial void Failed(this ILogger logger, Exception ex, double elapsed);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Created {Count} congregation(s) (1 big, {Small} small)."
    )]
    public static partial void CreatedCongregations(this ILogger logger, int count, int small);

    [LoggerMessage(Level = LogLevel.Information, Message = "Created {Count} admin user(s).")]
    public static partial void CreatedAdmins(this ILogger logger, int count);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Created lookup entities — labels={SessionLabels} assets={AssetCategories} projects={ProjectCategories} transactions={TransactionCategories}."
    )]
    public static partial void CreatedReferenceEntities(
        this ILogger logger,
        int sessionLabels,
        int assetCategories,
        int projectCategories,
        int transactionCategories
    );

    [LoggerMessage(Level = LogLevel.Information, Message = "{Congregation}: {Count} assets.")]
    public static partial void AssetsDone(this ILogger logger, string congregation, int count);
}
