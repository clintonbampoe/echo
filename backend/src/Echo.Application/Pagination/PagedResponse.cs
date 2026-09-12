namespace Echo.Application.Pagination;

public class PagedResponse<T>(bool hasMore, string? next, List<T> data)
{
    public bool HasMore { get; init; } = hasMore;
    public string? Next { get; init; } = next;
    public List<T> Data { get; init; } = data;
}
