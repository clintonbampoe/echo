using Echo.Application.Members;
using Echo.Domain.Members;
using Echo.Shared.Utilities;
using Microsoft.Extensions.Time.Testing;

namespace Echo.Application.Tests.Members;

[Trait("Category", "Unit")]
public class MemberWindowTests
{
    private static readonly DateWindows _windows = new(
        new FakeTimeProvider(DateTimeOffset.Parse("2026-10-14T10:00:00Z"))
    );

    private static Member JoinedOn(DateOnly? joined)
    {
        var member = TestData.Shared.CreateMember();
        member.JoinedDate = joined;
        return member;
    }

    [Fact]
    public void ResolveWindow_Default_IsThisYear()
    {
        var w = MemberService.ResolveWindow(new MemberFilters(), _windows);

        Assert.Equal(new DateWindow(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), true), w);
    }

    [Fact]
    public void Filter_FromAndTo_KeepsMembersWithNullJoinedDate()
    {
        var rows = new[]
        {
            JoinedOn(new DateOnly(2026, 5, 1)),
            JoinedOn(new DateOnly(2020, 5, 1)),
            JoinedOn(null),
        };
        var filters = new MemberFilters
        {
            From = new DateOnly(2026, 1, 1),
            To = new DateOnly(2026, 12, 31),
        };

        var res = rows.AsQueryable().Filter(filters).ToList();

        Assert.Equal(2, res.Count);
    }

    private static Member Named(string name, Guid id)
    {
        var member = TestData.Shared.CreateMember();
        member.Person = new Person { Name = name };
        member.PersonId = id;
        return member;
    }

    [Fact]
    public void Paginate_Cursor_KeepsMembersAfterItByNameThenId()
    {
        var cursor = new MemberCursor
        {
            Name = "Kofi",
            Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
        };
        var sameNameBefore = Named("Kofi", Guid.Parse("00000000-0000-0000-0000-000000000004"));
        var sameNameAfter = Named("Kofi", Guid.Parse("00000000-0000-0000-0000-000000000006"));
        var alphabeticallyBefore = Named("Ama", Guid.Parse("00000000-0000-0000-0000-000000000009"));
        var alphabeticallyAfter = Named("Yaw", Guid.Parse("00000000-0000-0000-0000-000000000001"));

        var res = new[] { sameNameBefore, sameNameAfter, alphabeticallyBefore, alphabeticallyAfter }
            .AsQueryable()
            .Paginate(cursor, 10)
            .ToList();

        Assert.Equal([sameNameAfter, alphabeticallyAfter], res);
    }

    [Fact]
    public void JoinedByMonth_CountsMembersByTheMonthTheyJoined_AndSkipsThoseWithNoJoinDate()
    {
        var rows = new[]
        {
            JoinedOn(new DateOnly(2026, 3, 2)),
            JoinedOn(new DateOnly(2026, 3, 30)),
            JoinedOn(new DateOnly(2026, 5, 9)),
            JoinedOn(null),
        };

        var res = rows.AsQueryable().JoinedByMonth().OrderBy(r => r.Month).ToList();

        Assert.Equal([new MemberMonthRow(2026, 3, 2), new MemberMonthRow(2026, 5, 1)], res);
    }

    [Fact]
    public void JoinedCounts_SplitsJoinsBetweenTheWindow_TheOneBeforeIt_AndThisYear()
    {
        var window = new DateWindow(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 14), false);
        var previous = window.Previous();
        var yearStart = new DateOnly(2026, 1, 1);
        var today = new DateOnly(2026, 10, 14);
        var rows = new[]
        {
            JoinedOn(new DateOnly(2026, 10, 3)),
            JoinedOn(new DateOnly(2026, 10, 14)),
            JoinedOn(new DateOnly(2026, 9, 20)),
            JoinedOn(new DateOnly(2026, 3, 1)),
            JoinedOn(new DateOnly(2025, 12, 31)),
            JoinedOn(null),
        };

        var res = rows.AsQueryable().JoinedCounts(window, previous, yearStart, today).Single();

        Assert.Equal((2, 1, 4), (res.InWindow, res.InPreviousWindow, res.ThisYear));
    }
}
