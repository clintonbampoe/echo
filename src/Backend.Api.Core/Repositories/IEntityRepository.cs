public interface IEntityRepository<T> where T : class
{
    Task<T> GetPageAsync(
        PaginationParams paginationParameters,
        QueryParameters queryParameters,
        CancellationToken cancellationToken = default
        );

    Task<T> GetByIdAsync(Guid Id, CancellationToken cancellationToken = default);

    Task<T> CreateRecord(T recordData, CancellationToken cancellationToken = default);

    Task<T> UpdateRecord(Guid Id, T recordData, CancellationToken cancellationToken = default);

    Task<T> DeleteRecord(Guid Id, CancellationToken cancellationToken = default);
}
