public interface IRepository<T> where T : class
{
    Task<T> GetPageAsync(PaginationParams paginationParameters, CancellationToken cancellationToken = default);

    Task<T> GetByIdAsync(Guid Id, CancellationToken cancellationToken = default);

    // TODO: Insert contract for GetByQueryParameters
    // This is where we can pass various parameters to accurately filter database response data

    Task<T> CreateRecord(T recordData, CancellationToken cancellationToken = default);

    Task<T> UpdateRecord(Guid Id, T recordData, CancellationToken cancellationToken = default);

    Task<T> DeleteRecord(Guid Id, CancellationToken cancellationToken = default);
}
