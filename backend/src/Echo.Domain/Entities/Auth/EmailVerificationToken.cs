using Echo.Domain.Entities.Auth.Interfaces;
using Echo.Domain.Entities.Core;

namespace Echo.Domain.Entities.Auth;

public class EmailVerificationToken(Guid userId) : IAuthEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7(DateTime.UtcNow);
    public Guid UserId { get; set; } = userId;
    public User User { get; set; } = null!;
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow + TimeSpan.FromHours(24);
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
