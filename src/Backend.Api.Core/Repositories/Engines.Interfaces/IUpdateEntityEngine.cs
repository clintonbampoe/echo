using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Repositories.Engines.Interfaces;

public interface IUpdateEntityEngine<T>
    where T : ICongregationEntity
{
    Task<bool> UpdateEntityById(
        Guid Id,
        T updatedRecordData,
        CancellationToken cancellationToken = default
    );
}
