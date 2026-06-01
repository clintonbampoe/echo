public class PagedResponse<T> where T : class
{
    public List<T> Data { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }

    public PagedResponse(List<T> data, PaginationParams paginationParams, int totalRecords)
    {
        Data = data;
        PageNumber = paginationParams.PageNumber;
        PageSize = paginationParams.PageSize;
        TotalRecords = totalRecords;
    }
}
