using Echo.Application.Members;
using Echo.Domain.Members;

namespace Echo.Application.Tests.Members;

[Trait("Category", "Unit")]
public class VisitorSourceTests
{
    private static readonly Guid Inviter = Guid.NewGuid();

    [Fact]
    public void SomeoneInvited_WithNoSourceGiven_IsRecordedAsInvited()
    {
        var (source, problem) = VisitorService.SourceFor(null, Inviter);

        Assert.Equal(VisitorSource.Invited, source);
        Assert.Null(problem);
    }

    [Fact]
    public void SomeoneInvited_CannotBeAWalkInOrOnline()
    {
        var (_, problem) = VisitorService.SourceFor(VisitorSource.WalkIn, Inviter);

        Assert.Contains("invited", problem, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(VisitorSource.WalkIn)]
    [InlineData(VisitorSource.Online)]
    [InlineData(VisitorSource.Invited)]
    public void WithNoInviter_TheSourceIsKeptAsGiven(VisitorSource? requested)
    {
        var (source, problem) = VisitorService.SourceFor(requested, null);

        Assert.Equal(requested, source);
        Assert.Null(problem);
    }

    [Fact]
    public void Filter_BySourceAndInviter_KeepsOnlyMatchingVisitors()
    {
        Visitor Visitor(VisitorSource? source, Guid? invitedBy)
        {
            var visitor = new Visitor
            {
                PersonId = Guid.NewGuid(),
                Person = TestData.Shared.CreatePerson(),
                CreatedAt = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                Source = source,
                InvitedByMemberId = invitedBy,
            };
            return visitor;
        }

        var walkIn = Visitor(VisitorSource.WalkIn, null);
        var byInviter = Visitor(VisitorSource.Invited, Inviter);
        var byOther = Visitor(VisitorSource.Invited, Guid.NewGuid());

        var all = new[] { walkIn, byInviter, byOther }.AsQueryable();

        Assert.Equal([walkIn], all.Filter(new VisitorFilters { Source = VisitorSource.WalkIn }).ToList());
        Assert.Equal([byInviter], all.Filter(new VisitorFilters { InvitedByMemberId = Inviter }).ToList());
    }
}
