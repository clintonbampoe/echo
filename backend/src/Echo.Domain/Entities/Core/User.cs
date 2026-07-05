using Echo.Domain.Entities.Core.Interfaces;
using Echo.Domain.Enums;

namespace Echo.Domain.Entities.Core;

public class User : IPrimaryEntity, ISearchableEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7(DateTime.UtcNow);
    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;

    public string EmailAddress { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
