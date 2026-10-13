using System.ComponentModel.DataAnnotations;
using Echo.Domain.Members;
using Echo.Domain.Users;

namespace Echo.Auth.Invitations;

public enum InvitationStatus
{
    Active,
    Expired,
    Revoked,
}

public record InviteRequest
{
    public required UserRole AllowedRole { get; init; }

    [Range(1, 30)]
    public int? ExpiryDays { get; init; }

    [StringLength(100)]
    public string? Label { get; init; }
}

public record InviteResponseDto
{
    public required Guid Id { get; init; }

    /// <summary>The invitation code, shown once, grouped like K7QX-92MD-PW4T.</summary>
    public required string Token { get; init; }
    public required UserRole AllowedRole { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public string? Label { get; init; }
}

public record InvitationUpdateDto
{
    /// <summary>Send an empty string to clear the label.</summary>
    [StringLength(100)]
    public string? Label { get; init; }

    /// <summary>Makes the invitation expire this many days from now.</summary>
    [Range(1, 30)]
    public int? ExpiryDays { get; init; }
}

public record InvitationFilters
{
    public InvitationStatus? Status { get; init; }
}

public record InvitationResponseDto
{
    public required Guid Id { get; init; }
    public string? Label { get; init; }
    public required UserRole AllowedRole { get; init; }
    public required string CreatedByName { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required InvitationStatus Status { get; init; }
    public required int UseCount { get; init; }
}

/// <summary>What the join screen shows before the person creates an account.</summary>
public record InvitationLookupDto
{
    public required string CongregationName { get; init; }
    public required string City { get; init; }
    public required Region Region { get; init; }
    public required UserRole AllowedRole { get; init; }
    public required DateTime ExpiresAt { get; init; }
}

public record InvitationCursor
{
    public DateTime CreatedAt { get; init; }
    public Guid Id { get; init; }
}
