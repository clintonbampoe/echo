using Echo.Data;
using Echo.Domain.Congregations;
using Echo.Domain.Transactions;

namespace Echo.Application;

// A summary reads all of its numbers in one query: each is a scalar subquery projected from a
// source that yields exactly one row (the caller's congregation). One round trip, however many
// numbers the summary has, and nothing runs concurrently on the DbContext.
public record AssetStats(int Count, decimal CurrentValue, decimal PurchaseCost);

public record MemberStats(int Total, int Active, int Male, int Female, double AverageAge);

public record VisitorStats(int Total, int Converted, int Recurring);

public record EventStats(int Total, int Upcoming, int Registrations, int Attendees);

public record ContributionStats(
    decimal Total,
    int Count,
    decimal Average,
    PaymentMethod? MostUsedPaymentMethod
);

public record ProjectStats(int Count, decimal Raised, decimal Target, int AtRisk);

public record TitheStats(
    decimal Total,
    int UniqueTithers,
    decimal Average,
    PaymentMethod? MostUsedPaymentMethod
);

public record OrganizationStats(int Count, int TotalMembers, double AverageMembers, string? Largest);

public record SessionStats(
    int Total,
    int Completed,
    int Cancelled,
    int TotalAttendance,
    int CompletedAttendance
);

public record AttendanceStats(int Total, int Members, int Visitors, int FirstTimeVisitors);

internal static class SummaryQueries
{
    internal static IQueryable<Congregation> OneRow(this AppDbContext context, Guid congregationId) =>
        context.Set<Congregation>().Where(c => c.Id == congregationId);
}
