using Echo.Application.Projects;
using Echo.Domain.Projects;
using Echo.Shared.Utilities;
using Microsoft.Extensions.Time.Testing;

namespace Echo.Application.Tests.Projects;

[Trait("Category", "Unit")]
public class ProjectWindowTests
{
    private static readonly DateWindows _windows = new(
        new FakeTimeProvider(DateTimeOffset.Parse("2026-10-14T10:00:00Z"))
    );

    private static Project Spanning(DateOnly start, DateOnly? end)
    {
        var project = ProjectFactory.NewEntity();
        project.StartDate = start;
        project.EndDate = end;
        return project;
    }

    private static List<Project> Default(params Project[] rows)
    {
        var w = ProjectService.ResolveWindow(new ProjectFilters(), _windows);
        var filters = new ProjectFilters { From = w?.From, To = w?.To };
        return rows.AsQueryable().Filter(filters).ToList();
    }

    [Fact]
    public void ResolveWindow_Default_IsThisYear()
    {
        var w = ProjectService.ResolveWindow(new ProjectFilters(), _windows);

        Assert.Equal(
            new DateWindow(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), true),
            w
        );
    }

    [Fact]
    public void Filter_Default_KeepsProjectStartedLastYearAndStillRunning()
    {
        var running = Spanning(new DateOnly(2025, 6, 1), new DateOnly(2027, 1, 31));

        Assert.Single(Default(running));
    }

    [Fact]
    public void Filter_Default_KeepsOpenEndedProjectStartedInPreviousYear()
    {
        var openEnded = Spanning(new DateOnly(2024, 3, 1), null);

        Assert.Single(Default(openEnded));
    }

    [Fact]
    public void Filter_Default_DropsProjectThatEndedBeforeTheWindow()
    {
        var finished = Spanning(new DateOnly(2024, 3, 1), new DateOnly(2025, 12, 31));

        Assert.Empty(Default(finished));
    }

    [Fact]
    public void Filter_Default_DropsProjectStartingAfterTheWindow()
    {
        var future = Spanning(new DateOnly(2027, 1, 1), null);

        Assert.Empty(Default(future));
    }

    [Fact]
    public void Filter_Default_KeepsProjectStartedThisYear()
    {
        var thisYear = Spanning(new DateOnly(2026, 4, 1), new DateOnly(2026, 6, 1));

        Assert.Single(Default(thisYear));
    }
}
