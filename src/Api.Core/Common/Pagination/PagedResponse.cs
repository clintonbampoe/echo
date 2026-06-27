namespace Api.Core.Common.Pagination;

public class PagedResponse<T>
    where T : class
{
    public List<T> Data { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecordCount { get; set; }

    public PagedResponse(List<T> data, PaginationParameters paginationParams, int totalRecords)
    {
        Data = data;
        PageNumber = paginationParams.PageNumber;
        PageSize = paginationParams.PageSize;
        TotalRecordCount = totalRecords;
    }
}
