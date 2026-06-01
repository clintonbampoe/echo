namespace Backend.Api.Core.Services.Interfaces;

public interface IGetRecordService<T> where T : class, ICongregationEntity
{
    Task<T?> GetRecordByIdAsync(Guid Id, CancellationToken cancellationToken = default);

    int GetTotalRecordsCount(QueryParameters queryParameters);

    PagedResponse<T> CreateNewPagedResponseObject(
        List<T> records, PaginationParams paginationParameters, int totalRecords);

}
