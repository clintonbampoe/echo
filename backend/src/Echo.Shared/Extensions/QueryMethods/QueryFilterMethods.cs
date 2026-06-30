using Echo.Domain.Entities.Core.Interfaces;
using Echo.Shared.Query;

namespace Echo.Shared.Extensions.QueryMethods;

public static class QueryFilterMethods
{
    public static IQueryable<T> ApplySearchFilter<T>(
        this IQueryable<T> query,
        QueryParameters? queryParameters
    )
        where T : class, ISearchableEntity
    {
        if (queryParameters is null)
            return query;

        if (string.IsNullOrWhiteSpace(queryParameters.SearchTerm))
            return query;

        return query.Where(x => x.Name.Contains(queryParameters.SearchTerm));
    }

    public static IQueryable<T> ApplyDateFilters<T>(
        this IQueryable<T> query,
        QueryParameters? queryParameters
    )
        where T : ICongregationEntity
    {
        if (queryParameters is null)
            return query;

        if (queryParameters.StartDate.HasValue)
            query = query.Where(x => x.CreatedAt >= queryParameters.StartDate.Value);

        if (queryParameters.EndDate.HasValue)
            query = query.Where(x => x.CreatedAt <= queryParameters.EndDate.Value);

        return query;
    }

    public static IQueryable<T> ApplySoftDeleteFilter<T>(this IQueryable<T> query)
        where T : ICongregationEntity
    {
        return query.Where(x => x.DeletedAt == null);
    }
}
