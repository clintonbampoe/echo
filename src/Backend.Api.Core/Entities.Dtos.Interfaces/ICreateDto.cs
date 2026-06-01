namespace Backend.Api.Core.Entities.Dtos.Interfaces;

public interface ICreateDto<T> where T : class, ICongregationEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
}
