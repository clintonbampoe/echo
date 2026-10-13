using Echo.Domain.Congregations;
using Echo.Domain.Users;

namespace Echo.Domain.Auth;

public class InvitationToken : ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;

    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public UserRole AllowedRole { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }

    // A name the admin gives the invitation ("Youth group link") so they can tell them apart. The
    // code itself is only stored as a hash and is shown once.
    public string? Label { get; set; }

    // How many people have registered with this invitation. It stays valid until it expires or is revoked.
    public int UseCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
