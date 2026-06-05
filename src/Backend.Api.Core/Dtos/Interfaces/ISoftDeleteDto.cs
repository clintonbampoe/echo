using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Dtos.Interfaces;

public interface ISoftDeleteDto<T> where T : class, ICongregationEntity, ISoftDeletableEntity
{
    Guid Id { get; init; }
    Guid CongregationId { get; init; }
}
