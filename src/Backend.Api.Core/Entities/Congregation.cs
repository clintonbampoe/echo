using Backend.Api.Core.Entities.Interfaces;

public class Congregation : ISoftDeletableEntity, ISearchableEntity, IDateTrackedEntity
{
    public Guid CongregationId { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow; 
    public string Name { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
