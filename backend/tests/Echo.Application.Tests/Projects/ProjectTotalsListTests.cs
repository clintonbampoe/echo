using Echo.Application.Projects;
using Echo.Domain.Projects;

namespace Echo.Application.Tests.Projects;

[Trait("Category", "Unit")]
public class ProjectTotalsListTests
{
    private static Project Ending(DateOnly? end) =>
        new()
        {
            Id = Guid.NewGuid(),
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = end,
        };

    [Fact]
    public void SettledIds_AreTheProjectsThatEndedBeforeYesterday_NotYesterdayOrLater()
    {
        var today = new DateOnly(2026, 11, 3);
        var old = Ending(today.AddDays(-200));
        var dayBefore = Ending(today.AddDays(-2));
        var yesterday = Ending(today.AddDays(-1));
        var now = Ending(today);
        var upcoming = Ending(today.AddDays(30));

        var settled = ProjectRepository.SettledIds([old, dayBefore, yesterday, now, upcoming], today);

        Assert.Equal([old.Id, dayBefore.Id], settled);
    }

    [Fact]
    public void SettledIds_LeavesAProjectWithNoEndDateOut_BecauseItIsStillGoing()
    {
        var ongoing = Ending(null);

        Assert.Empty(ProjectRepository.SettledIds([ongoing], new DateOnly(2026, 11, 3)));
    }
}
