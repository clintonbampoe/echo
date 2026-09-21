using Echo.Domain;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Query;

public static class Extensions
{
    public static IQueryable<T> FilterDeleted<T>(this IQueryable<T> query)
        where T : ISoftDeletable
    {
        return query.Where(x => x.DeletedAt == null);
    }

    public static IQueryable<T> SearchName<T>(this IQueryable<T> query, string name)
        where T : ISearchable
    {
        return query
            .Where(x => EF.Functions.ILike(x.Name, $"%{name}%"))
            .OrderByDescending(x => EF.Functions.TrigramsSimilarity(x.Name, name))
            .Take(5);
    }
}
