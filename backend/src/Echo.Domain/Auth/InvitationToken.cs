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

    // The code, encrypted, so the admin can copy it again later. Null for invitations made before
    // codes were kept: their code was only ever shown once.
    public string? EncryptedCode { get; set; }

    // Null means the invitation never expires.
    public DateTime? ExpiresAt { get; set; }

    // Null means any number of people may use it.
    public int? MaxUses { get; set; }
    public bool IsRevoked { get; set; }

    // A name the admin gives the invitation ("Youth group link") so they can tell them apart.
    public string? Label { get; set; }

    // How many people have registered with this invitation. It stays valid until it expires, is
    // revoked or reaches MaxUses.
    public int UseCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
