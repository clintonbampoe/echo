namespace Backend.Api.Core.Entities;

public class Organization : ICongregationEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; } = string.Empty;
    public DateOnly CreatedAt { get; init; }
}
