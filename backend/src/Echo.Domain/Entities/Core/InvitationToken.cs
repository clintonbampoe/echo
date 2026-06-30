using Echo.Domain.Entities.Core.Interfaces;
using Echo.Domain.Enums;

namespace Echo.Domain.Entities.Core;

public class InvitationToken : IPrimaryEntity
{
    public Guid Id { get; set; }
    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;
    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;
    public UserRole AllowedRole { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
