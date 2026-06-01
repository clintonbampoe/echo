namespace Backend.Api.Core.Entities.Dtos.Interfaces;

public interface IUpdateDto
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
}
