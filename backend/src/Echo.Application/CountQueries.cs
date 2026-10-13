using Echo.Application.Attendances;
using Echo.Domain.Assets;
using Echo.Domain.Attendances;
using Echo.Domain.Events;
using Echo.Domain.Members;
using Echo.Domain.Organizations;
using Echo.Domain.Projects;
using Echo.Domain.Transactions;
using Echo.Shared.Query;

namespace Echo.Application;

internal readonly record struct ParentCount(Guid Id, int Count);

internal readonly record struct CategoryCount(int Id, int Count);

internal readonly record struct ParentAmount(Guid Id, decimal Amount);

internal readonly record struct VisitStats(Guid PersonId, int Count, DateOnly LastDate);

/// <summary>
/// Grouped counts for the ids on one page of a list. Soft-deleted rows are never counted, which
/// matches the rules the category delete check and the summaries already use.
/// </summary>
internal static class CountQueries
{
    internal static IQueryable<ParentCount> CountPerEvent(
        this IQueryable<EventRegistration> query,
        IReadOnlyList<Guid> eventIds
    ) =>
        query
            .FilterDeleted()
            .Where(r => eventIds.Contains(r.EventId))
            .GroupBy(r => r.EventId)
            .Select(g => new ParentCount(g.Key, g.Count()));

    internal static IQueryable<ParentCount> CountPerOrganization(
        this IQueryable<OrganizationMember> query,
        IReadOnlyList<Guid> organizationIds
    ) =>
        query
            .FilterDeleted()
            .Where(m => organizationIds.Contains(m.OrganizationId))
            .GroupBy(m => m.OrganizationId)
            .Select(g => new ParentCount(g.Key, g.Count()));

    internal static IQueryable<ParentAmount> RaisedPerProject(
        this IQueryable<ProjectContribution> query,
        IReadOnlyList<Guid> projectIds
    ) =>
        query
            .FilterDeleted()
            .Where(c => projectIds.Contains(c.ProjectId))
            .GroupBy(c => c.ProjectId)
            .Select(g => new ParentAmount(g.Key, g.Sum(c => c.Amount)));

    internal static IQueryable<CategoryCount> CountPerProjectCategory(
        this IQueryable<Project> query,
        IReadOnlyList<int> categoryIds
    ) =>
        query
            .FilterDeleted()
            .Where(p => categoryIds.Contains(p.CategoryId))
            .GroupBy(p => p.CategoryId)
            .Select(g => new CategoryCount(g.Key, g.Count()));

    internal static IQueryable<CategoryCount> CountPerAssetCategory(
        this IQueryable<Asset> query,
        IReadOnlyList<int> categoryIds
    ) =>
        query
            .FilterDeleted()
            .Where(a => categoryIds.Contains(a.CategoryId))
            .GroupBy(a => a.CategoryId)
            .Select(g => new CategoryCount(g.Key, g.Count()));

    internal static IQueryable<CategoryCount> CountPerTransactionCategory(
        this IQueryable<Transaction> query,
        IReadOnlyList<int> categoryIds
    ) =>
        query
            .FilterDeleted()
            .Where(t => categoryIds.Contains(t.CategoryId))
            .GroupBy(t => t.CategoryId)
            .Select(g => new CategoryCount(g.Key, g.Count()));

    /// <summary>Visits are attendance records made while the person was a visitor.</summary>
    internal static IQueryable<VisitStats> VisitStatsFor(
        this IQueryable<Attendance> query,
        IReadOnlyList<Guid> personIds
    ) =>
        query
            .Live()
            .Where(a => personIds.Contains(a.PersonId) && a.PersonKindAtTime == PersonKind.Visitor)
            .GroupBy(a => a.PersonId)
            .Select(g => new VisitStats(g.Key, g.Count(), g.Max(a => a.Session.Date)));
}
