using Echo.Domain.Users;

namespace Echo.Domain.Auth;

public class PasswordResetToken : ISoftDeletable
{
    public Guid Id { get; set; } = Guid.CreateVersion7(DateTime.UtcNow);
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
