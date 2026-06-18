using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Entities;

public class Congregation : ISearchableEntity
{
    public Guid CongregationId { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}
