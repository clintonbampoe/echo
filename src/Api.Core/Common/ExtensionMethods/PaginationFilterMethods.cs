using System.Linq.Expressions;
using Api.Core.Common.Pagination;
using Api.Core.Dtos.Core.Interfaces;

namespace Api.Core.Common.ExtensionMethods;

public static class PaginationFilterMethods
{
    public static IQueryable<T> ApplyPagination<T>(
        this IQueryable<T> query,
        PaginationParameters paginationParams
    )
        where T : IPrimaryListResponseDto
    {
        if (paginationParams is null)
            return query;

        return query
            .OrderByGuidIfUnordered()
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize);
    }

    private static IQueryable<T> OrderByGuidIfUnordered<T>(this IQueryable<T> query)
        where T : IPrimaryListResponseDto
    {
        if (IsOrderedQuery(query))
            return query;

        return query.OrderBy(e => e.Id);
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
            if (
                call.Method.DeclaringType == typeof(Queryable)
                && (
                    name == "OrderBy"
                    || name == "OrderByDescending"
                    || name == "ThenBy"
                    || name == "ThenByDescending"
                )
            )
            {
                return true;
            }
            return HasOrderBy(call.Arguments[0]);
        }
        return false;
    }
}
