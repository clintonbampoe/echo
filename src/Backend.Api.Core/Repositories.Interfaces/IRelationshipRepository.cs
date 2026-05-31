namespace Backend.Api.Core;

public interface IRelationshipRepository<T> : IEntityRepository<T> where T : class
{
    public Task<IEnumerable<T>> GetPageByLeftEntityIdAsync(
        Guid leftId,
        PaginationParams paginationParameters,
        QueryParameters queryParameters
        );

    public Task<IEnumerable<T>> GetPageByRightEntityIdAsync(
        Guid rightId,
        PaginationParams paginationParameters,
        QueryParameters queryParameters
        );
}
