namespace Echo.Application.Query.Extensions;

public static class PaginationExtensions
{
    public static bool HasNextPage<T>(this List<T> items, int pageSize)
    {
        return items.Count > pageSize;
    }

    public static void TrimPage<T>(this List<T> items, int pageSize)
    {
        if (items.Count > pageSize)
        {
            var excess = items.Count - pageSize;
            items.RemoveRange(pageSize, excess);
        }
    }
}
