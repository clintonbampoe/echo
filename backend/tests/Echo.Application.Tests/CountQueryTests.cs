using Echo.Application.Assets;
using Echo.Application.Attendances;
using Echo.Application.Events;
using Echo.Application.Members;
using Echo.Application.Organizations;
using Echo.Application.Projects;
using Echo.Application.Tests.Assets;
using Echo.Application.Tests.Attendances;
using Echo.Application.Tests.Events;
using Echo.Application.Tests.Organizations;
using Echo.Application.Tests.Projects;
using Echo.Application.Tests.Transactions;
using Echo.Application.Transactions;
using Echo.Domain.Assets;
using Echo.Domain.Attendances;
using Echo.Domain.Events;
using Echo.Domain.Members;
using Echo.Domain.Organizations;
using Echo.Domain.Projects;
using Echo.Domain.Transactions;

namespace Echo.Application.Tests;

[Trait("Category", "Unit")]
public class CountQueryTests
{
    private static readonly Guid A = Guid.Parse("00000000-0000-0000-0000-00000000000a");
    private static readonly Guid B = Guid.Parse("00000000-0000-0000-0000-00000000000b");
    private static readonly Guid C = Guid.Parse("00000000-0000-0000-0000-00000000000c");

    [Fact]
    public void CountPerEvent_CountsLiveRegistrationsOfTheRequestedEventsOnly()
    {
        EventRegistration Reg(Guid eventId, bool deleted = false)
        {
            var r = TestData.Shared.CreateEventRegistration();
            r.Id = Guid.NewGuid();
            r.EventId = eventId;
            r.DeletedAt = deleted ? DateTime.UtcNow : null;
            return r;
        }

        var rows = new[] { Reg(A), Reg(A), Reg(A, deleted: true), Reg(B), Reg(C) };

        var res = rows.AsQueryable().CountPerEvent([A, B]).ToDictionary(x => x.Id, x => x.Count);

        Assert.Equal(new Dictionary<Guid, int> { [A] = 2, [B] = 1 }, res);
    }

    [Fact]
    public void CountPerOrganization_CountsLiveMembersOfTheRequestedOrganizationsOnly()
    {
        OrganizationMember Row(Guid organizationId, bool deleted = false)
        {
            var m = TestData.Shared.CreateOrganizationMember();
            m.Id = Guid.NewGuid();
            m.OrganizationId = organizationId;
            m.DeletedAt = deleted ? DateTime.UtcNow : null;
            return m;
        }

        var rows = new[] { Row(A), Row(A, deleted: true), Row(B), Row(B), Row(C) };

        var res = rows.AsQueryable()
            .CountPerOrganization([A, B])
            .ToDictionary(x => x.Id, x => x.Count);

        Assert.Equal(new Dictionary<Guid, int> { [A] = 1, [B] = 2 }, res);
    }

    [Fact]
    public void ContributionTotalsPerProject_SumsAndCountsLiveContributionsOfTheRequestedProjectsOnly()
    {
        ProjectContribution Row(Guid projectId, decimal amount, bool deleted = false)
        {
            var c = TestData.Shared.CreateProjectContribution();
            c.Id = Guid.NewGuid();
            c.ProjectId = projectId;
            c.Amount = amount;
            c.DeletedAt = deleted ? DateTime.UtcNow : null;
            return c;
        }

        var rows = new[]
        {
            Row(A, 100m),
            Row(A, 50.5m),
            Row(A, 1000m, deleted: true),
            Row(B, 20m),
            Row(C, 999m),
        };

        var res = rows.AsQueryable()
            .ContributionTotalsPerProject([A, B])
            .ToDictionary(x => x.Id, x => (x.Amount, x.Count));

        Assert.Equal(
            new Dictionary<Guid, (decimal, int)> { [A] = (150.5m, 2), [B] = (20m, 1) },
            res
        );
    }

    [Fact]
    public void CountPerProjectCategory_CountsLiveProjectsOfTheRequestedCategoriesOnly()
    {
        Project Row(int categoryId, bool deleted = false)
        {
            var p = TestData.Shared.CreateProject();
            p.Id = Guid.NewGuid();
            p.CategoryId = categoryId;
            p.DeletedAt = deleted ? DateTime.UtcNow : null;
            return p;
        }

        var rows = new[] { Row(1), Row(1), Row(1, deleted: true), Row(2), Row(3) };

        var res = rows.AsQueryable()
            .CountPerProjectCategory([1, 2])
            .ToDictionary(x => x.Id, x => x.Count);

        Assert.Equal(new Dictionary<int, int> { [1] = 2, [2] = 1 }, res);
    }

    [Fact]
    public void CountPerAssetCategory_CountsLiveAssetsOfTheRequestedCategoriesOnly()
    {
        Asset Row(int categoryId, bool deleted = false)
        {
            var a = TestData.Shared.CreateAsset();
            a.Id = Guid.NewGuid();
            a.CategoryId = categoryId;
            a.DeletedAt = deleted ? DateTime.UtcNow : null;
            return a;
        }

        var rows = new[] { Row(1), Row(1, deleted: true), Row(2), Row(2), Row(3) };

        var res = rows.AsQueryable()
            .CountPerAssetCategory([1, 2])
            .ToDictionary(x => x.Id, x => x.Count);

        Assert.Equal(new Dictionary<int, int> { [1] = 1, [2] = 2 }, res);
    }

    [Fact]
    public void CountPerTransactionCategory_CountsLiveTransactionsOfTheRequestedCategoriesOnly()
    {
        Transaction Row(int categoryId, bool deleted = false)
        {
            var t = TestData.Shared.CreateTransaction();
            t.Id = Guid.NewGuid();
            t.CategoryId = categoryId;
            t.DeletedAt = deleted ? DateTime.UtcNow : null;
            return t;
        }

        var rows = new[] { Row(1), Row(1), Row(2, deleted: true), Row(2), Row(3) };

        var res = rows.AsQueryable()
            .CountPerTransactionCategory([1, 2])
            .ToDictionary(x => x.Id, x => x.Count);

        Assert.Equal(new Dictionary<int, int> { [1] = 2, [2] = 1 }, res);
    }

    [Fact]
    public void VisitStatsFor_CountsLiveVisitorAttendanceAndFindsTheLatestSessionDate()
    {
        Attendance Row(
            Guid personId,
            DateOnly date,
            PersonKind kind = PersonKind.Visitor,
            bool deleted = false
        )
        {
            var session = TestData.Shared.CreateSession();
            session.Id = Guid.NewGuid();
            session.Date = date;

            var a = TestData.Shared.CreateAttendance();
            a.Id = Guid.NewGuid();
            a.PersonId = personId;
            a.PersonKindAtTime = kind;
            a.Session = session;
            a.SessionId = session.Id;
            a.SessionDate = date;
            a.DeletedAt = deleted ? DateTime.UtcNow : null;
            return a;
        }

        var rows = new[]
        {
            Row(A, new DateOnly(2026, 9, 6)),
            Row(A, new DateOnly(2026, 10, 11)),
            Row(A, new DateOnly(2026, 9, 20), deleted: true),
            Row(A, new DateOnly(2026, 10, 18), PersonKind.Member),
            Row(B, new DateOnly(2026, 8, 2)),
            Row(C, new DateOnly(2026, 10, 4)),
        };

        var res = rows.AsQueryable().VisitStatsFor([A, B]).ToDictionary(x => x.PersonId);

        Assert.Equal(2, res.Count);
        Assert.Equal((2, new DateOnly(2026, 10, 11)), (res[A].Count, res[A].LastDate));
        Assert.Equal((1, new DateOnly(2026, 8, 2)), (res[B].Count, res[B].LastDate));
    }
}
