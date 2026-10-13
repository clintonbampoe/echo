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
            PersonId = Constants.DefaultGuid,
            Person = PersonFactory.NewVisitorPerson(),
            CongregationId = Constants.DefaultGuid,
            CreatedAt = createdAt,
            ConvertedAt = convertedAt,
        };

    [Fact]
    public void ResolveWindow_Default_IsThisYear()
    {
        var w = VisitorService.ResolveWindow(new VisitorFilters(), _windows);

        Assert.Equal(
            new DateWindow(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), true),
            w
        );
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
}
