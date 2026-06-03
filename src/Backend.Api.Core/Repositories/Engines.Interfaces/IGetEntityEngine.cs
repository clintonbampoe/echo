namespace Backend.Api.Core.Repositories.Engines.Interfaces;

public interface IGetEntityEngine<T> where T : class, ICongregationEntity
{
    Task<T?> GetEntityByIdAsync(Guid Id, CancellationToken cancellationToken = default);

    PagedResponse<T> CreateNewPagedResponseObject(
        List<T> records, PaginationParams paginationParameters, int totalRecords);

}
