using Echo.Domain.Entities.Core;

namespace Echo.Domain.Entities.Auth;

public class PasswordVerificationToken : IAuthEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7(DateTime.UtcNow);
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
