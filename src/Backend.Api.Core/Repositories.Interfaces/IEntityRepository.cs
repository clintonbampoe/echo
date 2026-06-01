using Backend.Api.Core.Entities.Dtos.Interfaces;

namespace Backend.Api.Core.Repositories.Interfaces;

public interface IEntityRepository<T> where T : class, ICongregationEntity
{
    Task<IEnumerable<IResponseDto>> GetPageAsync(
        PaginationParams paginationParameters,
        QueryParameters queryParameters,
        CancellationToken cancellationToken = default
        );

    Task<IResponseDto> GetByIdAsync(Guid Id, CancellationToken cancellationToken = default);

    Task<IResponseDto> CreateRecord(ICreateDto recordData, CancellationToken cancellationToken = default);

    Task<bool> UpdateRecord(Guid Id, IUpdateDto recordData, CancellationToken cancellationToken = default);

    Task DeleteRecord(Guid Id, CancellationToken cancellationToken = default);
}
