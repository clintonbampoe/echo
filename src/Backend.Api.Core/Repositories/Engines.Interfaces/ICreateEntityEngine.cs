namespace Backend.Api.Core.Repositories.Engines.Interfaces;

public interface ICreateEntityEngine<T> where T : class, ICongregationEntity
{
    Task<bool> CreateNewEntity(T newRecordData, CancellationToken cancellationToken = default);
}
