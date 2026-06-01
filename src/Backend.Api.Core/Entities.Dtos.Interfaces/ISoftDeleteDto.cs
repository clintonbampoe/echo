using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Entities.Dtos;

public interface ISoftDeleteDto<T> where T : class, ICongregationEntity, ISoftDeletableEntity
{
    Guid Id { get; init; }
    Guid CongregationId { get; init; }
}
