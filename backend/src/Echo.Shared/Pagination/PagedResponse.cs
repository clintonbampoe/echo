using Echo.Shared.Utilities;

namespace Echo.Shared.Pagination;

public class PagedResponse<T>(bool hasMore, string? next, List<T> data, DateWindow? window = null)
{
    public bool HasMore { get; init; } = hasMore;
    public string? Next { get; init; } = next;
    public List<T> Data { get; init; } = data;
    public DateWindow? Window { get; init; } = window;
}
