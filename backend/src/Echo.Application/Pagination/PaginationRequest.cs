namespace Echo.Application.Pagination;

public class PaginationRequest
{
    public string? Cursor { get; init; }

    public int PageSize
    {
        get;
        set => field = Math.Clamp(value, 1, 24);
    } = 24;
}
