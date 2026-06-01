using Backend.Api.Core.Entities.Dtos.Interfaces;

namespace Backend.Api.Core.Repositories.Interfaces;

public interface IRelationshipRepository<T> : IEntityRepository<T> where T : class, ICongregationEntity
{
    public Task<PagedResponse<T>> GetPageByLeftEntityIdAsync(
        Guid leftId,
        PaginationParams paginationParameters,
        QueryParameters queryParameters
        );

    public Task<PagedResponse<T>> GetPageByRightEntityIdAsync(
        Guid rightId,
        PaginationParams paginationParameters,
        QueryParameters queryParameters
        );
}
