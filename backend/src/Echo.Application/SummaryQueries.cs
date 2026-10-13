using Echo.Data;
using Echo.Domain.Congregations;

namespace Echo.Application;

// A count of rows grouped by a parent's id (a Guid or an int). Plain values shared by the count
// queries that live in each module.
internal readonly record struct ParentCount(Guid Id, int Count);

internal readonly record struct CategoryCount(int Id, int Count);

// A summary reads all of its numbers in one query: each is a scalar subquery projected from a
// source that yields exactly one row (the caller's congregation). One round trip, however many
// numbers the summary has, and nothing runs concurrently on the DbContext.
internal static class SummaryQueries
{
    internal static IQueryable<Congregation> OneRow(this AppDbContext context, Guid congregationId) =>
        context.Set<Congregation>().Where(c => c.Id == congregationId);
}
