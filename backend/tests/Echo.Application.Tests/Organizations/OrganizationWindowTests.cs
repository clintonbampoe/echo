using Echo.Application.Organizations;
using Echo.Domain.Organizations;
using Echo.Shared.Utilities;
using Microsoft.Extensions.Time.Testing;

namespace Echo.Application.Tests.Organizations;

[Trait("Category", "Unit")]
public class OrganizationWindowTests
{
    private static readonly DateWindows _windows = new(
        new FakeTimeProvider(DateTimeOffset.Parse("2026-10-14T10:00:00Z"))
    );

    private static Organization CreatedOn(DateTime createdAt)
    {
        var organization = OrganizationFactory.NewEntity();
        organization.CreatedAt = createdAt;
        return organization;
    }

    [Fact]
    public void ResolveWindow_Default_IsThisYear()
    {
        var w = OrganizationService.ResolveWindow(new OrganizationFilters(), _windows);

        Assert.Equal(
            new DateWindow(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), true),
            w
        );
    }

    [Fact]
    public void Filter_FromAndTo_KeepsOnlyRowsInsideInclusive()
    {
        var rows = new[]
        {
            CreatedOn(new DateTime(2025, 12, 31, 23, 0, 0, DateTimeKind.Utc)),
            CreatedOn(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            CreatedOn(new DateTime(2026, 12, 31, 23, 0, 0, DateTimeKind.Utc)),
            CreatedOn(new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
        };
        var filters = new OrganizationFilters
        {
            From = new DateOnly(2026, 1, 1),
            To = new DateOnly(2026, 12, 31),
        };

        var res = rows.AsQueryable().Filter(filters).ToList();

        Assert.Equal(2, res.Count);
    }
}
