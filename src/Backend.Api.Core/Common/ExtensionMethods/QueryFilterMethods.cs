using Backend.Api.Core.Common.Query;
using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Common.ExtensionMethods;

public static class QueryFilterMethods
{
    public static IQueryable<T> ApplySoftDeleteFilter<T>(this IQueryable<T> query)
        where T : class, ISoftDeletableEntity
    {
        return query.Where(e => e.DeletedAt == null);
    }

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
}
