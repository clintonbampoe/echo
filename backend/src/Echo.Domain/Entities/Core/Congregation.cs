using Echo.Domain.Entities.Core.Interfaces;

namespace Echo.Domain.Entities.Core;

public class Congregation : ISearchableEntity
{
    public Guid CongregationId { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}
