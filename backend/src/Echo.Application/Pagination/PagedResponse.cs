namespace Echo.Application.Pagination;

public class PagedResponse<T>
{
    public required List<T> Data { get; init; }
    public bool HasMore { get; init; }
    public string? Next { get; init; }
}
