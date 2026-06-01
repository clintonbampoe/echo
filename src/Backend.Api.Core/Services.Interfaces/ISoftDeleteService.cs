using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Services.Interfaces;

public interface ISoftDeleteService<T> where T : class, ICongregationEntity, ISoftDeletableEntity
{
    Task<int> SoftDeleteByIdAsync(Guid Id, CancellationToken cancellationToken = default);
}
