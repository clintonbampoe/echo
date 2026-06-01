using System.Linq.Expressions;
using Backend.Api.Core.Entities.Dtos;
using Backend.Api.Core.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Common.ExtensionMethods;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, PaginationParams paginationParams)
        where T : class, ICongregationEntity
    {
        if (paginationParams is null)
            return query;

        return query
            .OrderByGuidIfUnordered()
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize);
    }

    public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> query, QueryParameters queryParameters)
        where T : class, ICongregationEntity, IDateTrackedEntity, ISearchableEntity
    {
        if (queryParameters is null)
            return query;

        query = ApplySearchTerm(query, queryParameters.SearchTerm);

        query = ApplyStartDateFilter(query, queryParameters.StartDate);

        query = ApplyEndDateFilter(query, queryParameters.EndDate);

        return query;
    }

    private static IQueryable<T> OrderByGuidIfUnordered<T>(this IQueryable<T> query)
        where T : class, ICongregationEntity
    {
        if (IsOrderedQuery(query))
            return query;

        return query.OrderBy(e => e.Id);
    }


    private static IQueryable<T> ApplySearchTerm<T>(this IQueryable<T> query, string? searchTerm)
        where T : class, ISearchableEntity
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        return query.Where(x => x.Name.Contains(searchTerm));
    }

    private static IQueryable<T> ApplyStartDateFilter<T>(this IQueryable<T> query, DateTime? startDate)
        where T : class, IDateTrackedEntity
    {
        if (!startDate.HasValue)
            return query;

        return query.Where(x => x.CreatedAt >= startDate.Value);
    }

    private static IQueryable<T> ApplyEndDateFilter<T>(this IQueryable<T> query, DateTime? endDate)
        where T : class, IDateTrackedEntity
    {
        if (!endDate.HasValue)
            return query;

        return query.Where(x => x.CreatedAt <= endDate.Value);
    }

    private static bool IsOrderedQuery<T>(IQueryable<T> query)
    {
        return HasOrderBy(query.Expression);
    }

    private static bool HasOrderBy(Expression expression)
    {
        if (expression is MethodCallExpression call)
        {
            string name = call.Method.Name;
            if (call.Method.DeclaringType == typeof(Queryable) &&
                (name == "OrderBy" || name == "OrderByDescending" ||
                name == "ThenBy" || name == "ThenByDescending"))
            {
                return true;
            }
            return HasOrderBy(call.Arguments[0]);
        }
        return false;
    }
}
