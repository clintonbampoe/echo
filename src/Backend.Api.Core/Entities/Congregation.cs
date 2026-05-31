using Backend.Api.Core.Entities.Interfaces;

public class Congregation : ISoftDeletableEntity
{
    public Guid CongregationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? DeletedAt { get; set; }
}
