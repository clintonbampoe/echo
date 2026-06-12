using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Entities;

public class Congregation : ISoftDeletableEntity, ISearchableEntity, IDateTrackedEntity
{
    public Guid CongregationId { get; set; } = Guid.CreateVersion7();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Name { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
