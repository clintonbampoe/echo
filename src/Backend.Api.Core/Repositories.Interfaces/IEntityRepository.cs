using Backend.Api.Core.Entities.Dtos.Interfaces;

namespace Backend.Api.Core.Repositories.Interfaces;

public interface IEntityRepository<T> where T : class, ICongregationEntity
{
    Task<PagedResponse<T>> GetPageAsync(
        PaginationParams paginationParameters,
        QueryParameters queryParameters,
        CancellationToken cancellationToken = default
        );

    Task<T?> GetByIdAsync(Guid Id, CancellationToken cancellationToken = default);

    Task<bool> CreateRecord(T newRecordData, CancellationToken cancellationToken = default);

    Task<bool> UpdateRecord(Guid Id, T updatedRecordData, CancellationToken cancellationToken = default);

    Task<bool> DeleteRecord(Guid Id, CancellationToken cancellationToken = default);
}
