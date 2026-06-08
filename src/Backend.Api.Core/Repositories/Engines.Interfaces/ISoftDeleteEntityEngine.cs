using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Repositories.Engines.Interfaces;

public interface ISoftDeleteEntityEngine<T>
    where T : ICongregationEntity, ISoftDeletableEntity
{
    Task<bool> SoftDeleteByIdAsync(Guid Id, CancellationToken cancellationToken = default);
}
