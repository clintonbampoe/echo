using Echo.Application.Members;
using Echo.Domain.Members;
using Echo.Shared.Utilities;
using Microsoft.Extensions.Time.Testing;

namespace Echo.Application.Tests.Members;

[Trait("Category", "Unit")]
public class VisitorWindowTests
{
    private static readonly DateWindows _windows = new(
        new FakeTimeProvider(DateTimeOffset.Parse("2026-10-14T10:00:00Z"))
    );

    private static Visitor NewVisitor(DateTime createdAt, DateTime? convertedAt = null) =>
        new()
        {
            PersonId = TestData.DefaultId,
            Person = TestData.Shared.CreatePerson(),
            CongregationId = TestData.DefaultId,
            CreatedAt = createdAt,
            ConvertedAt = convertedAt,
        };

    [Fact]
    public void ResolveWindow_Default_IsThisYear()
    {
        var w = VisitorService.ResolveWindow(new VisitorFilters(), _windows);

        Assert.Equal(new DateWindow(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), true), w);
    }

    [Fact]
    public void Filter_Default_KeepsUnconvertedVisitorFromLastMonth()
    {
        var w = VisitorService.ResolveWindow(new VisitorFilters(), _windows);
        var filters = new VisitorFilters { From = w?.From, To = w?.To };
        var lastMonth = NewVisitor(new DateTime(2026, 9, 5, 0, 0, 0, DateTimeKind.Utc));
        var lastYear = NewVisitor(new DateTime(2025, 9, 5, 0, 0, 0, DateTimeKind.Utc));

        var res = new[] { lastMonth, lastYear }.AsQueryable().Filter(filters).ToList();

        var only = Assert.Single(res);
        Assert.Equal(lastMonth.CreatedAt, only.CreatedAt);
    }

    [Fact]
    public void Filter_Default_ExcludesConvertedVisitors()
    {
        var converted = NewVisitor(
            new DateTime(2026, 9, 5, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 20, 0, 0, 0, DateTimeKind.Utc)
        );

        var res = new[] { converted }.AsQueryable().Filter(new VisitorFilters()).ToList();

        Assert.Empty(res);
    }

    [Fact]
    public void Filter_ConvertedView_WindowsByConvertedAtNotCreatedAt()
    {
        var convertedThisYear = NewVisitor(
            new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 20, 0, 0, 0, DateTimeKind.Utc)
        );
        var convertedLastYear = NewVisitor(
            new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 9, 20, 0, 0, 0, DateTimeKind.Utc)
        );
        var filters = new VisitorFilters
        {
            Converted = true,
            From = new DateOnly(2026, 1, 1),
            To = new DateOnly(2026, 12, 31),
        };

        var res = new[] { convertedThisYear, convertedLastYear }
            .AsQueryable()
            .Filter(filters)
            .ToList();

        var only = Assert.Single(res);
        Assert.Equal(convertedThisYear.ConvertedAt, only.ConvertedAt);
    }

    [Fact]
    public void Filter_To_KeepsTheWholeLastDayButNotTheNext()
    {
        var lastSecond = NewVisitor(new DateTime(2026, 3, 31, 23, 59, 59, DateTimeKind.Utc));
        var nextMidnight = NewVisitor(new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc));
        var filters = new VisitorFilters
        {
            From = new DateOnly(2026, 3, 1),
            To = new DateOnly(2026, 3, 31),
        };

        var res = new[] { lastSecond, nextMidnight }.AsQueryable().Filter(filters).ToList();

        Assert.Equal([lastSecond], res);
    }

    [Fact]
    public void Filter_FromOnTheFirstSecondOfTheDay_KeepsIt()
    {
        var firstSecond = NewVisitor(new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc));
        var dayBefore = NewVisitor(new DateTime(2026, 2, 28, 23, 59, 59, DateTimeKind.Utc));
        var filters = new VisitorFilters { From = new DateOnly(2026, 3, 1) };

        var res = new[] { firstSecond, dayBefore }.AsQueryable().Filter(filters).ToList();

        Assert.Equal([firstSecond], res);
    }

    [Fact]
    public void Filter_ToTheLastRepresentableDay_DoesNotOverflow()
    {
        var visitor = NewVisitor(new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc));
        var filters = new VisitorFilters { To = DateOnly.MaxValue };

        var res = new[] { visitor }.AsQueryable().Filter(filters).ToList();

        Assert.Single(res);
    }

    [Fact]
    public void Paginate_Cursor_KeepsVisitorsAfterItByNameThenId()
    {
        Visitor Named(string name, string id)
        {
            var visitor = NewVisitor(new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc));
            visitor.Person = new Person { Name = name };
            visitor.PersonId = Guid.Parse(id);
            return visitor;
        }

        var cursor = new VisitorCursor
        {
            Name = "Kofi",
            Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
        };
        var before = Named("Kofi", "00000000-0000-0000-0000-000000000004");
        var after = Named("Kofi", "00000000-0000-0000-0000-000000000006");
        var earlier = Named("Ama", "00000000-0000-0000-0000-000000000009");
        var later = Named("Yaw", "00000000-0000-0000-0000-000000000001");

        var res = new[] { before, after, earlier, later }
            .AsQueryable()
            .Paginate(cursor, 10)
            .ToList();

        Assert.Equal([after, later], res);
    }

    [Fact]
    public void Filter_ByGender_KeepsOnlyThatGender()
    {
        var woman = NewVisitor(new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc));
        woman.Gender = Gender.Female;
        var man = NewVisitor(new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc));
        man.Gender = Gender.Male;
        var unknown = NewVisitor(new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc));

        var res = new[] { woman, man, unknown }
            .AsQueryable()
            .Filter(new VisitorFilters { Gender = Gender.Female })
            .ToList();

        Assert.Equal([woman], res);
    }
}
