using Echo.Domain.Entities.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Extensions.QueryExtensions;

public static class SearchExtensions
{
    public static IQueryable<T> SearchName<T>(this IQueryable<T> query, string name)
        where T : ISearchableEntity
    {
        return query
            .Where(e => EF.Functions.ILike(e.Name, $"%{name}%"))
            .OrderByDescending(e => EF.Functions.TrigramsSimilarity(e.Name, name))
            .Take(5);
    }
}
