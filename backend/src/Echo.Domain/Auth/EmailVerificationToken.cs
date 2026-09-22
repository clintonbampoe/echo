using Echo.Domain.Users;

namespace Echo.Domain.Auth;

public class EmailVerificationToken(Guid userId) : ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } = userId;
    public User User { get; set; } = null!;

    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow + TimeSpan.FromHours(24);
    public DateTime? UsedAt { get; set; }
    public DateTime? InvalidatedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
