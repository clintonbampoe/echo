using System.Linq.Expressions;
using Backend.Api.Core.Entities.Dtos;
using Backend.Api.Core.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Common.ExtensionMethods;

public static class QueryFilterMethods
{
    public static IQueryable<T> ApplySearchFilter<T>(this IQueryable<T> query, QueryParameters queryParameters)
        where T : class, ISearchableEntity
    {
        if (string.IsNullOrWhiteSpace(queryParameters.SearchTerm))
            return query;

        return query.Where(x => x.Name.Contains(queryParameters.SearchTerm));
    }

    public static IQueryable<T> ApplyDateFilters<T>(this IQueryable<T> query, QueryParameters queryParameters)
        where T : class, IDateTrackedEntity
    {
        if (queryParameters.StartDate.HasValue)
            query = query.Where(x => x.CreatedAt >= queryParameters.StartDate.Value);

        if (queryParameters.EndDate.HasValue)
            query = query.Where(x => x.CreatedAt <= queryParameters.EndDate.Value);

        return query;
    }
}
