using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Services.Interfaces;

public interface ISoftDeleteEntityEngine<T> where T : class, ICongregationEntity, ISoftDeletableEntity
{
    Task<bool> SoftDeleteByIdAsync(Guid Id, CancellationToken cancellationToken = default);
}
