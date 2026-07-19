using Echo.Domain.Entities.Core;

namespace Echo.Domain.Entities.Auth;

public class RefreshToken(Guid userId) : IAuthEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7(DateTime.UtcNow);
    public Guid UserId { get; set; } =  userId;
    public User User { get; set; } = null!;

    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow + TimeSpan.FromDays(30);
    public DateTime? RevokedAt { get; set; }
    public Guid? ReplacedByTokenId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

