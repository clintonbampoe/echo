using Echo.Application.Assets;
using Echo.Application.Attendances;
using Echo.Application.Events;
using Echo.Application.Members;
using Echo.Application.Organizations;
using Echo.Application.Projects;
using Echo.Application.Tests.Assets;
using Echo.Application.Tests.Attendances;
using Echo.Application.Tests.Congregations;
using Echo.Application.Tests.Events;
using Echo.Application.Tests.Members;
using Echo.Application.Tests.Organizations;
using Echo.Application.Tests.Projects;
using Echo.Application.Tests.Tithes;
using Echo.Application.Tests.Transactions;
using Echo.Application.Tithes;
using Echo.Application.Transactions;
using Echo.Domain.Assets;
using Echo.Domain.Attendances;
using Echo.Domain.Congregations;
using Echo.Domain.Events;
using Echo.Domain.Members;
using Echo.Domain.Organizations;
using Echo.Domain.Projects;
using Echo.Domain.Tithes;
using Echo.Domain.Transactions;

namespace Echo.Application.Tests;

// Each summary reads all of its numbers in one query by projecting scalar subqueries from a
// one-row source. These tests run the same projection in memory.
[Trait("Category", "Unit")]
public class SummaryStatsTests
{
    private static readonly IQueryable<Congregation> OneRow = new[]
    {
        CongregationFactory.NewEntity(),
    }.AsQueryable();

    [Fact]
    public void Assets_SumValueAndCostAndCount()
    {
        Asset Row(decimal value, decimal cost)
        {
            var a = AssetFactory.NewEntity();
            a.CurrentValue = value;
            a.PurchaseCost = cost;
            return a;
        }

        var res = OneRow.SummarizeAssets(new[] { Row(100m, 150m), Row(50m, 80m) }.AsQueryable()).Single();

        Assert.Equal(new AssetStats(2, 150m, 230m), res);
    }

    [Fact]
    public void Assets_NoAssets_AreAllZero()
    {
        var res = OneRow.SummarizeAssets(Array.Empty<Asset>().AsQueryable()).Single();

        Assert.Equal(new AssetStats(0, 0m, 0m), res);
    }

    [Fact]
    public void Members_CountByStatusAndGender_AndAverageTheAge()
    {
        Member Row(MemberStatus status, Gender gender, int birthYear)
        {
            var m = MemberFactory.NewEntity();
            m.Status = status;
            m.Gender = gender;
            m.DateOfBirth = new DateOnly(birthYear, 6, 1);
            return m;
        }

        var members = new[]
        {
            Row(MemberStatus.Active, Gender.Male, 2000),
            Row(MemberStatus.Active, Gender.Female, 1990),
            Row(MemberStatus.Inactive, Gender.Female, 1980),
        };

        var res = OneRow.SummarizeMembers(members.AsQueryable(), currentYear: 2026).Single();

        Assert.Equal(new MemberStats(3, 2, 1, 2, (26 + 36 + 46) / 3.0), res);
    }

    [Fact]
    public void Members_NoMembers_AverageAgeIsZero()
    {
        var res = OneRow.SummarizeMembers(Array.Empty<Member>().AsQueryable(), 2026).Single();

        Assert.Equal(new MemberStats(0, 0, 0, 0, 0d), res);
    }

    [Fact]
    public void Visitors_CountTotalConvertedAndRecurring()
    {
        var recurringPerson = Guid.NewGuid();
        var oncePerson = Guid.NewGuid();
        var convertedPerson = Guid.NewGuid();
        Visitor Visitor(Guid personId, DateTime? convertedAt = null) =>
            new()
            {
                PersonId = personId,
                Person = PersonFactory.NewVisitorPerson(),
                CongregationId = Constants.DefaultGuid,
                ConvertedAt = convertedAt,
            };
        Attendance Present(Guid personId)
        {
            var a = AttendanceFactory.NewEntity();
            a.Id = Guid.NewGuid();
            a.PersonId = personId;
            a.Session.Id = Guid.NewGuid();
            return a;
        }

        var visitors = new[]
        {
            Visitor(recurringPerson),
            Visitor(oncePerson),
            Visitor(convertedPerson, DateTime.UtcNow),
        };
        var attendance = new[] { Present(recurringPerson), Present(recurringPerson), Present(oncePerson) };

        var res = OneRow.SummarizeVisitors(visitors.AsQueryable(), attendance.AsQueryable().Live()).Single();

        Assert.Equal(new VisitorStats(3, 1, 1), res);
    }

    [Fact]
    public void Events_CountTotalUpcomingRegistrationsAndAttendees()
    {
        var today = new DateOnly(2026, 10, 14);
        Event Row(DateOnly start)
        {
            var e = EventFactory.NewEntity();
            e.Id = Guid.NewGuid();
            e.StartDate = start;
            return e;
        }

        var past = Row(new DateOnly(2026, 9, 1));
        var upcoming = Row(new DateOnly(2026, 11, 1));
        EventRegistration Reg(Guid eventId)
        {
            var r = EventRegistrationFactory.NewEntity();
            r.Id = Guid.NewGuid();
            r.EventId = eventId;
            return r;
        }
        EventAttendance Attended(Guid eventId)
        {
            var a = EventAttendanceFactory.NewEntity();
            a.Id = Guid.NewGuid();
            a.EventId = eventId;
            return a;
        }

        var res = OneRow
            .SummarizeEvents(
                new[] { past, upcoming }.AsQueryable(),
                new[] { Reg(past.Id), Reg(upcoming.Id), Reg(upcoming.Id), Reg(Guid.NewGuid()) }.AsQueryable(),
                new[] { Attended(past.Id), Attended(Guid.NewGuid()) }.AsQueryable(),
                today
            )
            .Single();

        Assert.Equal(new EventStats(2, 1, 3, 1), res);
    }

    [Fact]
    public void Contributions_SumCountAverageAndFindTheMostUsedMethod()
    {
        ProjectContribution Row(decimal amount, PaymentMethod method)
        {
            var c = ProjectContributionFactory.NewEntity();
            c.Amount = amount;
            c.PaymentMethod = method;
            return c;
        }

        var rows = new[]
        {
            Row(100m, PaymentMethod.Cash),
            Row(50m, PaymentMethod.MobileMoney),
            Row(30m, PaymentMethod.MobileMoney),
        };

        var res = OneRow.SummarizeContributions(rows.AsQueryable()).Single();

        Assert.Equal(new ContributionStats(180m, 3, 60m, PaymentMethod.MobileMoney), res);
    }

    [Fact]
    public void Contributions_NoContributions_AreZeroWithNoMethod()
    {
        var res = OneRow.SummarizeContributions(Array.Empty<ProjectContribution>().AsQueryable()).Single();

        Assert.Equal(new ContributionStats(0m, 0, 0m, null), res);
    }

    [Fact]
    public void Projects_SumRaisedAndTargetAndCountAtRisk()
    {
        Project Row(decimal target, ProjectStatus status)
        {
            var p = ProjectFactory.NewEntity();
            p.Id = Guid.NewGuid();
            p.TargetAmount = target;
            p.Status = status;
            return p;
        }
        ProjectContribution Gave(Guid projectId, decimal amount)
        {
            var c = ProjectContributionFactory.NewEntity();
            c.ProjectId = projectId;
            c.Amount = amount;
            return c;
        }

        var a = Row(1000m, ProjectStatus.OnTrack);
        var b = Row(500m, ProjectStatus.AtRisk);
        var contributions = new[] { Gave(a.Id, 200m), Gave(a.Id, 100m), Gave(b.Id, 50m), Gave(Guid.NewGuid(), 999m) };

        var res = OneRow.SummarizeProjects(new[] { a, b }.AsQueryable(), contributions.AsQueryable()).Single();

        Assert.Equal(new ProjectStats(2, 350m, 1500m, 1), res);
    }

    [Fact]
    public void Tithes_SumUniqueTithersAverageAndFindTheMostUsedMethod()
    {
        var member1 = Guid.NewGuid();
        var member2 = Guid.NewGuid();
        Tithe Row(Guid memberId, decimal amount, PaymentMethod method)
        {
            var t = TitheFactory.NewEntity();
            t.MemberId = memberId;
            t.Amount = amount;
            t.PaymentMethod = method;
            return t;
        }

        var rows = new[]
        {
            Row(member1, 100m, PaymentMethod.Cash),
            Row(member1, 50m, PaymentMethod.Cash),
            Row(member2, 30m, PaymentMethod.Cheque),
        };

        var res = OneRow.SummarizeTithes(rows.AsQueryable()).Single();

        Assert.Equal(new TitheStats(180m, 2, 60m, PaymentMethod.Cash), res);
    }

    [Fact]
    public void Tithes_NoTithes_AreZeroWithNoMethod()
    {
        var res = OneRow.SummarizeTithes(Array.Empty<Tithe>().AsQueryable()).Single();

        Assert.Equal(new TitheStats(0m, 0, 0m, null), res);
    }

    [Fact]
    public void Organizations_CountMembersAverageAndFindTheLargest()
    {
        Organization Org(string name)
        {
            var o = OrganizationFactory.NewEntity();
            o.Id = Guid.NewGuid();
            o.Name = name;
            return o;
        }
        OrganizationMember Joined(Guid organizationId)
        {
            var m = OrganizationMemberFactory.NewEntity();
            m.Id = Guid.NewGuid();
            m.OrganizationId = organizationId;
            return m;
        }

        var choir = Org("Choir");
        var youth = Org("Youth");
        var members = new[] { Joined(choir.Id), Joined(choir.Id), Joined(choir.Id), Joined(youth.Id) };

        var res = OneRow.SummarizeOrganizations(new[] { choir, youth }.AsQueryable(), members.AsQueryable()).Single();

        Assert.Equal(new OrganizationStats(2, 4, 2.0, "Choir"), res);
    }

    [Fact]
    public void Organizations_NoOrganizations_AreZeroWithNoLargest()
    {
        var res = OneRow
            .SummarizeOrganizations(Array.Empty<Organization>().AsQueryable(), Array.Empty<OrganizationMember>().AsQueryable())
            .Single();

        Assert.Equal(new OrganizationStats(0, 0, 0d, null), res);
    }

    [Fact]
    public void Transactions_TotalsAndMostActiveCategoryComeFromTheCategoryRows()
    {
        var rows = new List<TransactionCategoryTotalDto>
        {
            new() { CategoryId = 1, CategoryName = "Offerings", Type = TransactionType.Income, Total = 900m, Count = 3 },
            new() { CategoryId = 2, CategoryName = "Utilities", Type = TransactionType.Expense, Total = 300m, Count = 5 },
            new() { CategoryId = 3, CategoryName = "Repairs", Type = TransactionType.Expense, Total = 100m, Count = 2 },
        };

        var res = TransactionService.BuildSummary(rows);

        Assert.Equal((900m, 400m, "Utilities"), (res.TotalIncome, res.TotalExpenses, res.MostActiveCategory));
        Assert.Equal(500m, res.Net);
        Assert.Equal(3, res.ByCategory.Count);
    }

    [Fact]
    public void Transactions_NoRows_AreZeroWithNoCategory()
    {
        var res = TransactionService.BuildSummary([]);

        Assert.Equal((0m, 0m, null), (res.TotalIncome, res.TotalExpenses, res.MostActiveCategory));
        Assert.Empty(res.ByCategory);
    }

    [Fact]
    public void Sessions_CountByStatus_AndSumAttendanceOnAllAndOnCompleted()
    {
        Session Session(SessionStatus status)
        {
            var s = SessionFactory.NewEntity();
            s.Id = Guid.NewGuid();
            s.Status = status;
            return s;
        }
        Attendance Present(Session session)
        {
            var a = AttendanceFactory.NewEntity();
            a.Id = Guid.NewGuid();
            a.Session = session;
            a.SessionId = session.Id;
            return a;
        }

        var completed1 = Session(SessionStatus.Completed);
        var completed2 = Session(SessionStatus.Completed);
        var scheduled = Session(SessionStatus.Scheduled);
        var cancelled = Session(SessionStatus.Cancelled);
        var attendance = new[]
        {
            Present(completed1), Present(completed1),
            Present(completed2), Present(completed2), Present(completed2), Present(completed2),
            Present(cancelled),
        };

        var res = OneRow
            .SummarizeSessions(new[] { completed1, completed2, scheduled, cancelled }.AsQueryable(), attendance.AsQueryable().Live())
            .Single();

        Assert.Equal(new SessionStats(4, 2, 1, 7, 6), res);
    }

    [Fact]
    public void Attendance_CountsTotalAndKinds_AndOnlyVisitorsWithNoEarlierVisit()
    {
        var returning = Guid.NewGuid();
        var firstTimer = Guid.NewGuid();
        var member = Guid.NewGuid();
        Attendance Row(Guid personId, PersonKind kind, DateOnly date)
        {
            var session = SessionFactory.NewEntity();
            session.Id = Guid.NewGuid();
            session.Date = date;
            var a = AttendanceFactory.NewEntity();
            a.Id = Guid.NewGuid();
            a.PersonId = personId;
            a.PersonKindAtTime = kind;
            a.Session = session;
            a.SessionId = session.Id;
            return a;
        }

        var earlier = Row(returning, PersonKind.Visitor, new DateOnly(2026, 10, 4));
        var inWindow = new[]
        {
            Row(returning, PersonKind.Visitor, new DateOnly(2026, 10, 11)),
            Row(firstTimer, PersonKind.Visitor, new DateOnly(2026, 10, 11)),
            Row(member, PersonKind.Member, new DateOnly(2026, 10, 11)),
        };
        var allLive = inWindow.Append(earlier).ToArray();

        var res = OneRow.SummarizeAttendance(inWindow.AsQueryable(), allLive.AsQueryable().Live()).Single();

        Assert.Equal(new AttendanceStats(3, 1, 2, 1), res);
    }
}
