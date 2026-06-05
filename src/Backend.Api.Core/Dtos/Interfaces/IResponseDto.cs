using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Dtos.Interfaces;

public interface IResponseDto<T> where T : class, ICongregationEntity
{
    public Guid Id { get; init; }
}
