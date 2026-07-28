using Echo.Domain.Entities.Core;
using Echo.Domain.Enums;

namespace Echo.Domain.Entities.Auth;

public class InvitationToken : IAuthEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7(DateTime.UtcNow);
    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;

    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public UserRole AllowedRole { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
