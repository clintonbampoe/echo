namespace Backend.Api.Core.Services.Interfaces;

public interface ICreateEntityEngine<T> where T : class, ICongregationEntity
{
    Task<bool> CreateNewEntity(T newRecordData, CancellationToken cancellationToken = default);
}
