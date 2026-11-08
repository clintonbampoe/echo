using System.Text.RegularExpressions;

namespace Echo.Application.Tests.Caching;

// The cache is only right while two rules hold, and nothing in the compiler enforces them. These tests read the
// source so that breaking a rule fails the build instead of showing up as an answer that is quietly out of date.
[Trait("Category", "Unit")]
public partial class ReadCacheGuardTests
{
    private static string Source(string project)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "backend", "src")))
            dir = dir.Parent;
        return Path.Combine(dir!.FullName, "backend", "src", project);
    }

    private static IEnumerable<string> Files(string project) =>
        Directory
            .EnumerateFiles(Source(project), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

    private static IEnumerable<string> Where(string pattern, params string[] projects) =>
        projects
            .SelectMany(Files)
            .Where(f => Regex.IsMatch(File.ReadAllText(f), pattern))
            .Select(Path.GetFileName)
            .OfType<string>()
            .Order();

    [GeneratedRegex(@"ExecuteUpdateAsync|ExecuteDeleteAsync|ExecuteSql")]
    private static partial Regex BulkWrite();

    // A bulk statement changes rows without the save interceptor seeing it, so whoever runs one must drop the
    // cached answers themselves. A file that runs one without doing so must be named here, with the reason.
    private static readonly Dictionary<string, string> BulkWritesThatNeedNoDrop = new()
    {
        ["EventRegistrationRepository.cs"] = "only locks the event row; the registration is saved with the tracked save",
        ["InvitationRepository.cs"] = "counts a use inside the registration transaction, whose user insert drops the cache",
        ["SessionRepository.cs"] = "login tokens: no read shows them",
        ["PasswordResetRepository.cs"] = "login tokens: no read shows them",
        ["EmailVerificationRepository.cs"] = "login tokens: no read shows them",
    };

    [Fact]
    public void EveryBulkWrite_DropsTheCache_OrSaysWhyItNeedsNot()
    {
        var strays = Where(BulkWrite().ToString(), "Echo.Application", "Echo.Auth")
            .Where(f => !BulkWritesThatNeedNoDrop.ContainsKey(f))
            .Where(f =>
                !Files("Echo.Application")
                    .Concat(Files("Echo.Auth"))
                    .Single(p => Path.GetFileName(p) == f)
                    .Pipe(File.ReadAllText)
                    .Contains("cache.Drop(")
            )
            .ToList();

        Assert.True(
            strays.Count == 0,
            $"These run a bulk statement without dropping the cached answers: {string.Join(", ", strays)}. "
                + "Call ReadCacheEvictions.Drop after it commits, or add the file to BulkWritesThatNeedNoDrop with the reason."
        );
    }

    // Answers built from a materialized view change when the view is refreshed, not when something is written, so
    // they have to be in the group that the refresher drops (ReadCache.ViewPolicy).
    private static readonly string[] ReadersOfViews =
    [
        "AttendanceRepository.cs",
        "EventRepository.cs",
        "ProjectRepository.cs",
        "SessionRepository.cs",
    ];

    private static readonly string[] GroupsThatReadViews =
    [
        "AttendanceEndpoints.cs",
        "AttendanceSessionEndpoints.cs",
        "EventEndpoints.cs",
        "ProjectEndpoints.cs",
    ];

    [Fact]
    public void OnlyTheKnownRepositories_ReadAMaterializedView()
    {
        var readers = Where(
                @"Set<(SessionAttendanceTotals|EventCounts|ProjectTotals)>",
                "Echo.Application"
            )
            .ToList();

        Assert.Equal(ReadersOfViews, readers);
    }

    [Fact]
    public void TheGroupsThatReadAView_UseThePolicyTheRefresherDrops()
    {
        var wrong = Files("Echo.Application")
            .Where(f => GroupsThatReadViews.Contains(Path.GetFileName(f)))
            .Where(f => !File.ReadAllText(f).Contains("CacheOutput(ReadCache.ViewPolicy)"))
            .Select(Path.GetFileName)
            .ToList();

        Assert.Empty(wrong);
    }
}

internal static class PipeExtension
{
    public static TOut Pipe<TIn, TOut>(this TIn value, Func<TIn, TOut> map) => map(value);
}
