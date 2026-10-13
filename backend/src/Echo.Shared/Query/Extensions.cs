using Echo.Domain;
using Microsoft.EntityFrameworkCore;

namespace Echo.Shared.Query;

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
            .Where(x => EF.Functions.ILike(x.Name, $"%{EscapeLike(name)}%"))
            .OrderByDescending(x => EF.Functions.TrigramsSimilarity(x.Name, name))
            .Take(5);
    }

    /// <summary>
    /// Escapes the characters that LIKE treats as wildcards (and the escape character itself),
    /// so a search term is matched literally. PostgreSQL's default escape character is a backslash.
    /// </summary>
    public static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
