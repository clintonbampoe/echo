using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Repositories.Engines.Interfaces;

public interface IGetEntityEngine<T>
    where T : ICongregationEntity
{
    Task<T?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResponse<T>> GetPagedEntityListByIdAsync(Guid id, CancellationToken ct = default);
    PagedResponse<T> CreateNewPagedResponseObject(
        List<T> records,
        PaginationParameters paginationParameters,
        int totalRecords
    );
}
