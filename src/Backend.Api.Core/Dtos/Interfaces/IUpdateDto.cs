using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Dtos.Interfaces;

public interface IUpdateDto<T>
    where T : ICongregationEntity
{
    public Guid CongregationId { get; init; }
}
