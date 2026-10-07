namespace Echo.Shared.Pagination;

public class PaginationRequest(string? cursor, int pageSize = 50)
{
    public string? Cursor { get; } = cursor;
    public int PageSize { get; } = Math.Clamp(pageSize, 1, 50);
}
