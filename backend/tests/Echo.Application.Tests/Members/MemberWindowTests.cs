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
        var member = MemberFactory.NewEntity();
        member.JoinedDate = joined;
        return member;
    }

    [Fact]
    public void ResolveWindow_Default_IsThisYear()
    {
        var w = MemberService.ResolveWindow(new MemberFilters(), _windows);

        Assert.Equal(
            new DateWindow(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), true),
            w
        );
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
}
