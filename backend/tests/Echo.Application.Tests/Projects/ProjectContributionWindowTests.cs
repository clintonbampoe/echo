using Echo.Application.Projects;
using Echo.Domain.Projects;
using Echo.Shared.Utilities;
using Microsoft.Extensions.Time.Testing;

namespace Echo.Application.Tests.Projects;

[Trait("Category", "Unit")]
public class ProjectContributionWindowTests
{
    private static readonly DateWindows _windows = new(
        new FakeTimeProvider(DateTimeOffset.Parse("2026-10-14T10:00:00Z"))
    );

    private static ProjectContribution On(DateOnly dateContributed)
    {
        var contribution = TestData.Shared.CreateProjectContribution();
        contribution.DateContributed = dateContributed;
        return contribution;
    }

    [Fact]
    public void Filter_NoDates_AppliesNoDateRestriction()
    {
        var rows = new[] { On(new DateOnly(2020, 1, 1)), On(new DateOnly(2026, 10, 14)) };

        var res = rows.AsQueryable().Filter(new ProjectContributionFilters()).ToList();

        Assert.Equal(2, res.Count);
    }

    [Fact]
    public void Filter_FromAndTo_KeepsOnlyRowsInsideInclusive()
    {
        var rows = new[]
        {
            On(new DateOnly(2026, 9, 30)),
            On(new DateOnly(2026, 10, 1)),
            On(new DateOnly(2026, 10, 14)),
            On(new DateOnly(2026, 10, 15)),
        };
        var filters = new ProjectContributionFilters
        {
            From = new DateOnly(2026, 10, 1),
            To = new DateOnly(2026, 10, 14),
        };

        var res = rows.AsQueryable().Filter(filters).ToList();

        Assert.Equal(2, res.Count);
    }

    [Fact]
    public void ResolveWindow_Unscoped_DefaultsToThisMonthToDate()
    {
        var w = ProjectContributionService.ResolveWindow(
            new ProjectContributionFilters(),
            _windows
        );

        Assert.Equal(
            new DateWindow(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 14), true),
            w
        );
    }

    [Fact]
    public void ResolveWindow_ProjectIdSet_ReturnsNull()
    {
        var filters = new ProjectContributionFilters { ProjectId = TestData.DefaultId };

        Assert.Null(ProjectContributionService.ResolveWindow(filters, _windows));
    }
}
