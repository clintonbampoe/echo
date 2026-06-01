namespace Backend.Api.Core.Entities.Dtos.Interfaces;

public interface ICreateDto
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
}
