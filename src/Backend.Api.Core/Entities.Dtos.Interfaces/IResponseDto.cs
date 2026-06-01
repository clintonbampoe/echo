namespace Backend.Api.Core.Entities.Dtos.Interfaces;

public interface IResponseDto<T> where T : class, ICongregationEntity
{
    public Guid Id { get; init; }
}
