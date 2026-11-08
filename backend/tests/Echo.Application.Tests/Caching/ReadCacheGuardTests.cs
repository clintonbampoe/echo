using System.Text.RegularExpressions;

namespace Echo.Application.Tests.Caching;

// The cache is only right while two rules hold, and the compiler enforces neither. These tests read the source,
// so that breaking a rule fails the build instead of leaving an answer that is quietly out of date.
[Trait("Category", "Unit")]
public class ReadCacheGuardTests
{
    private static readonly Regex BulkWrite = new(@"ExecuteUpdateAsync|ExecuteDeleteAsync|ExecuteSql");

    private static readonly Regex ViewRead = new(
        @"Set<(SessionAttendanceTotals|EventCounts|ProjectTotals)>"
    );

    // A bulk statement changes rows without the save interceptor seeing it, so whoever runs one must invalidate
    // the cache themselves. A file that runs one without doing so is named here, with the reason.
    private static readonly Dictionary<string, string> BulkWritesThatNeedNoInvalidation = new()
    {
        ["EventRegistrationRepository.cs"] = "only locks the event; the registration is saved by the tracked save",
        ["InvitationRepository.cs"] = "counts a use inside the registration transaction, whose user insert invalidates",
        ["SessionRepository.cs"] = "login tokens: no read shows them",
        ["PasswordResetRepository.cs"] = "login tokens: no read shows them",
        ["EmailVerificationRepository.cs"] = "login tokens: no read shows them",
    };

    // Answers built from a materialized view go stale when the view is refreshed, not when something is written,
    // so the groups that serve them must use the policy the refresher invalidates.
    private static readonly string[] RepositoriesThatReadViews =
    [
        "AttendanceRepository.cs",
        "EventRepository.cs",
        "ProjectRepository.cs",
        "SessionRepository.cs",
    ];

    private static readonly string[] GroupsThatServeViews =
    [
        "AttendanceEndpoints.cs",
        "AttendanceSessionEndpoints.cs",
        "EventEndpoints.cs",
        "ProjectEndpoints.cs",
    ];

    private static List<(string Name, string Text)> Sources(params string[] projects)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "backend", "src")))
            dir = dir.Parent;

        var built = $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}";
        return projects
            .SelectMany(p =>
                Directory.EnumerateFiles(
                    Path.Combine(dir!.FullName, "backend", "src", p),
                    "*.cs",
                    SearchOption.AllDirectories
                )
            )
            .Where(file => !file.Contains(built))
            .Select(file => (Path.GetFileName(file), File.ReadAllText(file)))
            .ToList();
    }

    [Fact]
    public void EveryBulkWrite_InvalidatesTheCache_OrSaysWhyItNeedsNot()
    {
        var strays = Sources("Echo.Application", "Echo.Auth")
            .Where(f => BulkWrite.IsMatch(f.Text))
            .Where(f => !BulkWritesThatNeedNoInvalidation.ContainsKey(f.Name))
            .Where(f => !f.Text.Contains("cache.Invalidate"))
            .Select(f => f.Name)
            .ToList();

        Assert.True(
            strays.Count == 0,
            $"These run a bulk statement without invalidating the cached answers: {string.Join(", ", strays)}. "
                + "Call ReadCacheInvalidator.Invalidate after it commits, or add the file to "
                + "BulkWritesThatNeedNoInvalidation with the reason."
        );
    }

    [Fact]
    public void OnlyTheKnownRepositories_ReadAMaterializedView()
    {
        var readers = Sources("Echo.Application")
            .Where(f => ViewRead.IsMatch(f.Text))
            .Select(f => f.Name)
            .Order();

        Assert.Equal(RepositoriesThatReadViews, readers);
    }

    [Fact]
    public void TheGroupsThatServeAView_UseThePolicyTheRefresherInvalidates()
    {
        var wrong = Sources("Echo.Application")
            .Where(f => GroupsThatServeViews.Contains(f.Name))
            .Where(f => !f.Text.Contains("CacheOutput(ReadCache.ViewPolicy)"))
            .Select(f => f.Name);

        Assert.Empty(wrong);
    }
}
