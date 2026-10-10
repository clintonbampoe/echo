// Core/SeederLog.cs
namespace Echo.Seeder.Core;

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
        Message = "{Congregation}: generating {Members} members and {Visitors} visitors."
    )]
    public static partial void GeneratingPeople(
        this ILogger logger,
        string congregation,
        int members,
        int visitors
    );

    [LoggerMessage(Level = LogLevel.Information, Message = "  members: {N}/{Total}")]
    public static partial void MembersProgress(this ILogger logger, int n, int total);

    [LoggerMessage(Level = LogLevel.Information, Message = "  visitors: {N}/{Total}")]
    public static partial void VisitorsProgress(this ILogger logger, int n, int total);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "{Congregation}: people done — {Members} members, {Visitors} visitors."
    )]
    public static partial void PeopleDone(
        this ILogger logger,
        string congregation,
        int members,
        int visitors
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "{Congregation}: no members, skipping {Phase}."
    )]
    public static partial void SkippingPhase(
        this ILogger logger,
        string congregation,
        string phase
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "{Congregation}: {Orgs} orgs, {Assignments} memberships."
    )]
    public static partial void OrgsDone(
        this ILogger logger,
        string congregation,
        int orgs,
        int assignments
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "  {Congregation} attendance: {N}/{Total}"
    )]
    public static partial void AttendanceProgress(
        this ILogger logger,
        string congregation,
        int n,
        int total
    );

    [LoggerMessage(Level = LogLevel.Information, Message = "  tithes: {N}")]
    public static partial void TithesProgress(this ILogger logger, int n);

    [LoggerMessage(Level = LogLevel.Information, Message = "  transactions: {N}")]
    public static partial void TransactionsProgress(this ILogger logger, int n);

    [LoggerMessage(Level = LogLevel.Information, Message = "  contributions: {N}")]
    public static partial void ContributionsProgress(this ILogger logger, int n);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "{Congregation}: {Projects} projects, {Contributions} contributions."
    )]
    public static partial void FinancesDone(
        this ILogger logger,
        string congregation,
        int projects,
        int contributions
    );

    [LoggerMessage(Level = LogLevel.Information, Message = "{Congregation}: {Count} assets.")]
    public static partial void AssetsDone(this ILogger logger, string congregation, int count);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Reset requested — truncating all seeded tables."
    )]
    public static partial void Resetting(this ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Created {Count} congregation(s) (1 big, {Small} small)."
    )]
    public static partial void CongregationsCreated(this ILogger logger, int count, int small);

    [LoggerMessage(Level = LogLevel.Information, Message = "Created {Count} admin user(s).")]
    public static partial void AdminsCreated(this ILogger logger, int count);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Created lookup entities — attendance={A} assets={B} projects={P} transactions={T}."
    )]
    public static partial void LookupsCreated(this ILogger logger, int a, int b, int p, int t);
}
