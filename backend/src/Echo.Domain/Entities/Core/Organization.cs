using Echo.Domain.Entities.Core.Interfaces;

namespace Echo.Domain.Entities.Core;

public class Organization : IPrimaryEntity, ISearchableEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7(DateTime.UtcNow);
    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
