using Backend.Api.Core.Entities.Dtos.Interfaces;

namespace Backend.Api.Core.Repositories.Interfaces;

public interface IEntityRepository<T> where T : class, ICongregationEntity
{
    Task<PagedResponse<IResponseDto<T>>> GetPageAsync(
        PaginationParams paginationParameters,
        QueryParameters queryParameters,
        CancellationToken cancellationToken = default
        );

    Task<IResponseDto<T>?> GetByIdAsync(Guid Id, CancellationToken cancellationToken = default);

    Task<bool> CreateRecord(ICreateDto<T> recordData, CancellationToken cancellationToken = default);

    Task<bool> UpdateRecord(Guid Id, IUpdateDto<T> recordData, CancellationToken cancellationToken = default);

    Task DeleteRecord(Guid Id, CancellationToken cancellationToken = default);
}
