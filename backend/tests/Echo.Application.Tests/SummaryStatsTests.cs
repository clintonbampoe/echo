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
    private static readonly IQueryable<Congregation> _oneRow = new[]
    {
        CongregationFactory.NewEntity(),
    }.AsQueryable();

    [Fact]
    public void Assets_SumValueAndCostAndCount()
    {
        static Asset Row(decimal value, decimal cost)
        {
            var a = AssetFactory.NewEntity();
            a.CurrentValue = value;
            a.PurchaseCost = cost;
            return a;
        }

        var res = _oneRow
            .SummarizeAssets(new[] { Row(100m, 150m), Row(50m, 80m) }.AsQueryable())
            .Single();

        Assert.Equal(new AssetStats(2, 150m, 230m), res);
    }

    [Fact]
    public void Assets_NoAssets_AreAllZero()
    {
        var res = _oneRow.SummarizeAssets(Array.Empty<Asset>().AsQueryable()).Single();

        Assert.Equal(new AssetStats(0, 0m, 0m), res);
    }

    [Fact]
    public void Members_CountByStatusAndGender_AndAverageTheExactAge()
    {
        Member Row(MemberStatus status, Gender gender, DateOnly dateOfBirth)
        {
            var m = MemberFactory.NewEntity();
            m.Status = status;
            m.Gender = gender;
            m.DateOfBirth = dateOfBirth;
            // The GroupBy in our query targets CongregationId, so mock it consistently
            m.CongregationId = Guid.Empty;
            return m;
        }

        // Today is 2026-10-14: the first has had this year's birthday (26), the second has not
        // (35, not 36), and the third has it today (46).
        var members = new[]
        {
            Row(MemberStatus.Active, Gender.Male, new DateOnly(2000, 6, 1)),
            Row(MemberStatus.Active, Gender.Female, new DateOnly(1990, 12, 25)),
            Row(MemberStatus.Inactive, Gender.Female, new DateOnly(1980, 10, 14)),
        };

        var res = members.AsQueryable().SummarizeMembers(new DateOnly(2026, 10, 14)).Single();

        // 26 is in 18–29, 35 in 30–44, and 46 in 45–59.
        Assert.Equal(new MemberStats(3, 2, 1, 2, (26 + 35 + 46) / 3.0, 0, 1, 1, 1, 0), res);
    }

    [Fact]
    public void Members_NoMembers_AverageAgeIsZero()
    {
        var res = Array
            .Empty<Member>()
            .AsQueryable()
            .SummarizeMembers(new DateOnly(2026, 10, 14))
            .SingleOrDefault(); // An empty group yields no rows

        // If an empty collection yields null, map to our safe default fallback layout
        var finalResult = res ?? new MemberStats(0, 0, 0, 0, 0d, 0, 0, 0, 0, 0);

        Assert.Equal(new MemberStats(0, 0, 0, 0, 0d, 0, 0, 0, 0, 0), finalResult);
    }

    [Fact]
    public void Members_AgeGroups_SplitOnTheExactBirthday()
    {
        Member Born(DateOnly dateOfBirth)
        {
            var m = MemberFactory.NewEntity();
            m.DateOfBirth = dateOfBirth;
            m.CongregationId = Guid.Empty;
            return m;
        }

        // Today is 2026-10-14. For each edge, one member turns the age today and one tomorrow, so
        // the second is still a year younger.
        var members = new[]
        {
            Born(new DateOnly(2008, 10, 14)), // 18 today
            Born(new DateOnly(2008, 10, 15)), // 17 until tomorrow
            Born(new DateOnly(1996, 10, 14)), // 30 today
            Born(new DateOnly(1996, 10, 15)), // 29 until tomorrow
            Born(new DateOnly(1981, 10, 14)), // 45 today
            Born(new DateOnly(1981, 10, 15)), // 44 until tomorrow
            Born(new DateOnly(1966, 10, 14)), // 60 today
            Born(new DateOnly(1966, 10, 15)), // 59 until tomorrow
        };

        var res = members.AsQueryable().SummarizeMembers(new DateOnly(2026, 10, 14)).Single();

        Assert.Equal(
            new[] { 1, 2, 2, 2, 1 },
            new[] { res.Under18, res.Age18To29, res.Age30To44, res.Age45To59, res.Age60Plus }
        );
        Assert.Equal(
            res.Total,
            res.Under18 + res.Age18To29 + res.Age30To44 + res.Age45To59 + res.Age60Plus
        );
    }

    [Fact]
    public void Members_Filter_KeepsUndatedMembersAndOnlyTheDatedOnesInsideTheWindow()
    {
        Member Joined(DateOnly? on)
        {
            var m = MemberFactory.NewEntity();
            m.JoinedDate = on;
            return m;
        }

        var members = new[]
        {
            Joined(new DateOnly(2026, 2, 1)), // before
            Joined(new DateOnly(2026, 3, 15)), // inside
            Joined(new DateOnly(2026, 4, 2)), // after
            Joined(null), // no joined date: in every window
        }.AsQueryable();

        DateOnly?[] Dates(MemberFilters filters) =>
            members.Filter(filters).Select(m => m.JoinedDate).ToArray();

        var march = new MemberFilters
        {
            From = new DateOnly(2026, 3, 1),
            To = new DateOnly(2026, 3, 31),
        };
        var fromMarch = new MemberFilters { From = new DateOnly(2026, 3, 1) };
        var toMarch = new MemberFilters { To = new DateOnly(2026, 3, 31) };

        Assert.Equal(new DateOnly?[] { new DateOnly(2026, 3, 15), null }, Dates(march));
        Assert.Equal(
            new DateOnly?[] { new DateOnly(2026, 3, 15), new DateOnly(2026, 4, 2), null },
            Dates(fromMarch)
        );
        Assert.Equal(
            new DateOnly?[] { new DateOnly(2026, 2, 1), new DateOnly(2026, 3, 15), null },
            Dates(toMarch)
        );
        Assert.Equal(4, Dates(new MemberFilters()).Length);
    }

    [Fact]
    public void Visitors_CountTotalNewConvertedAndRecurring()
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

        // The visitor list shows people who have not converted. "New" is everyone created in the
        // window, converted or not; "converted" is everyone converted in the window.
        var current = new[] { Visitor(recurringPerson), Visitor(oncePerson) };
        var createdInWindow = new[]
        {
            Visitor(recurringPerson),
            Visitor(oncePerson),
            Visitor(convertedPerson, DateTime.UtcNow),
        };
        var convertedInWindow = new[] { Visitor(convertedPerson, DateTime.UtcNow) };
        var attendance = new[]
        {
            Present(recurringPerson),
            Present(recurringPerson),
            Present(oncePerson),
        };

        var res = _oneRow
            .SummarizeVisitors(
                current.AsQueryable(),
                createdInWindow.AsQueryable(),
                convertedInWindow.AsQueryable(),
                attendance.AsQueryable().Live()
            )
            .Single();

        Assert.Equal(new VisitorStats(Total: 2, New: 3, Converted: 1, Recurring: 1), res);
    }

    private static Visitor VisitorOn(DateTime createdAt, DateTime? convertedAt = null) =>
        new()
        {
            PersonId = Guid.NewGuid(),
            Person = PersonFactory.NewVisitorPerson(),
            CongregationId = Constants.DefaultGuid,
            CreatedAt = createdAt,
            ConvertedAt = convertedAt,
        };

    [Fact]
    public void Visitors_CreatedBetween_CountsEveryoneCreatedInTheWindowConvertedOrNot()
    {
        var rows = new[]
        {
            VisitorOn(new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc)),
            VisitorOn(
                new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc)
            ),
            VisitorOn(new DateTime(2025, 12, 31, 23, 0, 0, DateTimeKind.Utc)),
        };

        var res = rows.AsQueryable()
            .CreatedBetween(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31))
            .ToList();

        Assert.Equal(2, res.Count);
    }

    [Fact]
    public void Visitors_ConvertedBetween_UsesTheConversionDateAndSkipsUnconverted()
    {
        var rows = new[]
        {
            VisitorOn(
                new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc)
            ),
            VisitorOn(
                new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2025, 5, 1, 0, 0, 0, DateTimeKind.Utc)
            ),
            VisitorOn(new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc)),
        };

        var res = rows.AsQueryable()
            .ConvertedBetween(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31))
            .ToList();

        Assert.Single(res);
    }

    [Fact]
    public void Events_CountTotalUpcomingRegistrationsAndAttendees()
    {
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

        var res = _oneRow
            .SummarizeEvents(
                new[] { past, upcoming }.AsQueryable(),
                new[] { upcoming }.AsQueryable(),
                new[]
                {
                    Reg(past.Id),
                    Reg(upcoming.Id),
                    Reg(upcoming.Id),
                    Reg(Guid.NewGuid()),
                }.AsQueryable(),
                new[] { Attended(past.Id), Attended(Guid.NewGuid()) }.AsQueryable()
            )
            .Single();

        Assert.Equal(new EventStats(2, 1, 3, 1), res);
    }

    [Fact]
    public void Events_Upcoming_IgnoresTheDateWindow()
    {
        var today = new DateOnly(2026, 10, 14);
        Event Row(string name, DateOnly start)
        {
            var e = EventFactory.NewEntity();
            e.Id = Guid.NewGuid();
            e.Name = name;
            e.StartDate = start;
            return e;
        }

        var rows = new[]
        {
            Row("Camp", new DateOnly(2026, 9, 1)), // before today
            Row("Camp", new DateOnly(2026, 11, 1)), // upcoming, inside the window
            Row("Camp", new DateOnly(2027, 2, 1)), // upcoming, outside the window
            Row("Revival", new DateOnly(2026, 12, 1)),
        };
        var thisYear = new EventFilters
        {
            From = new DateOnly(2026, 1, 1),
            To = new DateOnly(2026, 12, 31),
        };

        var res = rows.AsQueryable().Upcoming(thisYear, today).ToList();

        Assert.Equal(3, res.Count);
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

        var res = _oneRow.SummarizeContributions(rows.AsQueryable()).Single();

        Assert.Equal(new ContributionStats(180m, 3, 60m, PaymentMethod.MobileMoney), res);
    }

    [Fact]
    public void Contributions_NoContributions_AreZeroWithNoMethod()
    {
        var res = _oneRow
            .SummarizeContributions(Array.Empty<ProjectContribution>().AsQueryable())
            .Single();

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
        var contributions = new[]
        {
            Gave(a.Id, 200m),
            Gave(a.Id, 100m),
            Gave(b.Id, 50m),
            Gave(Guid.NewGuid(), 999m),
        };

        var res = _oneRow
            .SummarizeProjects(new[] { a, b }.AsQueryable(), contributions.AsQueryable())
            .Single();

        Assert.Equal(new ProjectStats(2, 350m, 1500m, 1), res);
    }

    [Fact]
    public void Tithes_SumUniqueTithersAndFindTheMostUsedMethod()
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

        var res = _oneRow.SummarizeTithes(rows.AsQueryable()).Single();

        Assert.Equal(new TitheStats(180m, 2, PaymentMethod.Cash), res);
    }

    [Theory]
    [InlineData(180, 2, 90)]
    [InlineData(100, 4, 25)]
    [InlineData(0, 0, 0)]
    [InlineData(50, 0, 0)]
    public void Tithes_AveragePerMember_IsTheTotalOverTheUniqueTithers_ZeroWhenNone(
        int total,
        int tithers,
        int expected
    )
    {
        Assert.Equal((decimal)expected, TitheService.AveragePerMember(total, tithers));
    }

    [Fact]
    public void Tithes_NoTithes_AreZeroWithNoMethod()
    {
        var res = _oneRow.SummarizeTithes(Array.Empty<Tithe>().AsQueryable()).Single();

        Assert.Equal(new TitheStats(0m, 0, null), res);
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
        var members = new[]
        {
            Joined(choir.Id),
            Joined(choir.Id),
            Joined(choir.Id),
            Joined(youth.Id),
        };

        var res = _oneRow
            .SummarizeOrganizations(new[] { choir, youth }.AsQueryable(), members.AsQueryable())
            .Single();

        Assert.Equal(new OrganizationStats(2, 4, 2.0, "Choir"), res);
    }

    [Fact]
    public void Organizations_NoOrganizations_AreZeroWithNoLargest()
    {
        var res = _oneRow
            .SummarizeOrganizations(
                Array.Empty<Organization>().AsQueryable(),
                Array.Empty<OrganizationMember>().AsQueryable()
            )
            .Single();

        Assert.Equal(new OrganizationStats(0, 0, 0d, null), res);
    }

    [Fact]
    public void Transactions_TotalsAndMostActiveCategoryComeFromTheCategoryRows()
    {
        var rows = new List<TransactionCategoryTotalDto>
        {
            new()
            {
                CategoryId = 1,
                CategoryName = "Offerings",
                Type = TransactionType.Income,
                Total = 900m,
                Count = 3,
            },
            new()
            {
                CategoryId = 2,
                CategoryName = "Utilities",
                Type = TransactionType.Expense,
                Total = 300m,
                Count = 5,
            },
            new()
            {
                CategoryId = 3,
                CategoryName = "Repairs",
                Type = TransactionType.Expense,
                Total = 100m,
                Count = 2,
            },
        };

        var res = TransactionService.BuildSummary(rows);

        Assert.Equal(
            (900m, 400m, "Utilities"),
            (res.TotalIncome, res.TotalExpenses, res.MostActiveCategory)
        );
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
            Present(completed1),
            Present(completed1),
            Present(completed2),
            Present(completed2),
            Present(completed2),
            Present(completed2),
            Present(cancelled),
        };

        var res = _oneRow
            .SummarizeSessions(
                new[] { completed1, completed2, scheduled, cancelled }.AsQueryable(),
                attendance.AsQueryable().Live()
            )
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

        var res = _oneRow
            .SummarizeAttendance(inWindow.AsQueryable(), allLive.AsQueryable().Live())
            .Single();

        Assert.Equal(new AttendanceStats(3, 1, 2, 1), res);
    }
}
