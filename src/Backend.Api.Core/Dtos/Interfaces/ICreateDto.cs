using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Dtos.Interfaces;

public interface ICreateDto<T> where T : class, ICongregationEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
}
