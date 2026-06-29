using Api.Core.Entities.Auth.Interfaces;
using Api.Core.Entities.Core;

namespace Api.Core.Entities.Auth;

public class PasswordVerificationToken : IAuthEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
