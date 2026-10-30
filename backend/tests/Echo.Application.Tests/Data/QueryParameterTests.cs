using Echo.Application.Attendances;

namespace Echo.Application.Tests.Data;

// A record bound from the query string with `[AsParameters]` treats every property that is not nullable as a REQUIRED
// parameter, whatever default it has: a request without it is refused with 400 "Required parameter ... was not
// provided". So an optional filter must be a nullable type. This has bitten twice (`months`, `includePrevious`); this
// test keeps it from a third time.
[Trait("Category", "Unit")]
public class QueryParameterTests
{
    [Fact]
    public void EveryFilterRecord_HasOnlyOptionalProperties()
    {
        var offenders = typeof(AttendanceFilters)
            .Assembly.GetTypes()
            .Where(t => t.IsPublic && t.IsClass && t.Name.EndsWith("Filters", StringComparison.Ordinal))
            .SelectMany(t =>
                t.GetProperties()
                    .Where(p => p.SetMethod is { IsPublic: true })
                    .Where(p => p.PropertyType.IsValueType && Nullable.GetUnderlyingType(p.PropertyType) is null)
                    .Select(p => $"{t.Name}.{p.Name} ({p.PropertyType.Name})")
            )
            .ToList();

        Assert.True(
            offenders.Count == 0,
            "These filter properties are required query parameters; make them nullable: " + string.Join(", ", offenders)
        );
    }
}
