using Echo.Domain.Users;

namespace Echo.Domain.Auth;

public class RefreshToken(Guid userId) : ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } = userId;
    public User User { get; set; } = null!;

    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow + TimeSpan.FromDays(30);
    public DateTime? RevokedAt { get; set; }
    public Guid? ReplacedByTokenId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
