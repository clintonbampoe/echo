using Echo.Domain.Entities.Core.Interfaces;

namespace Echo.Application.Query.Extensions;

public static class FilterExtensions
{
    public static IQueryable<T> ApplySearchFilter<T>(
        this IQueryable<T> query,
        Parameters? queryParameters
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
        Parameters? queryParameters
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

    public static IQueryable<T> FilterSoftDeleted<T>(this IQueryable<T> query)
        where T : ISoftDeletable
    {
        return query.Where(x => x.DeletedAt == null);
    }
}
