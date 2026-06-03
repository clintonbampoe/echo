namespace Backend.Api.Core.Repositories.Engines.Interfaces;

public interface IUpdateEntityEngine<T> where T : class, ICongregationEntity
{
    Task<bool> UpdateEntityById(
        Guid Id, T updatedRecordData, CancellationToken cancellationToken = default);
}
