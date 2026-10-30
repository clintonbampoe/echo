using System.ComponentModel.DataAnnotations;
using Echo.Domain.Members;
using Echo.Domain.Users;

namespace Echo.Auth.Invitations;

public enum InvitationStatus
{
    Active,
    Expired,
    Revoked,

    /// <summary>Everyone it allowed has already joined.</summary>
    Exhausted,
}

public record InviteRequest
{
    public required UserRole AllowedRole { get; init; }

    /// <summary>Days until it expires, 1 to 365. Defaults to 30. Do not send it with <see cref="NeverExpires"/>.</summary>
    [Range(1, InvitationService.MaxExpiryDays)]
    public int? ExpiryDays { get; init; }

    /// <summary>True for an invitation that never expires.</summary>
    public bool NeverExpires { get; init; }

    /// <summary>How many people may join with it. Leave out for no limit.</summary>
    [Range(1, InvitationService.MaxUsesLimit)]
    public int? MaxUses { get; init; }

    [StringLength(100)]
    public string? Label { get; init; }
}

public record InviteByEmailRequest
{
    [Required, EmailAddress, StringLength(255)]
    public required string EmailAddress { get; init; }

    public required UserRole AllowedRole { get; init; }

    [Range(1, InvitationService.MaxExpiryDays)]
    public int? ExpiryDays { get; init; }

    public bool NeverExpires { get; init; }

    /// <summary>Defaults to the email address.</summary>
    [StringLength(100)]
    public string? Label { get; init; }
}

public record InviteResponseDto
{
    public required Guid Id { get; init; }

    /// <summary>The invitation code, grouped like K7QX-92MD-PW4T.</summary>
    public required string Token { get; init; }
    public required UserRole AllowedRole { get; init; }

    /// <summary>Null when the invitation never expires.</summary>
    public DateTime? ExpiresAt { get; init; }

    /// <summary>Null when any number of people may join.</summary>
    public int? MaxUses { get; init; }
    public string? Label { get; init; }

    /// <summary>The address the invitation was emailed to; null when it was only created.</summary>
    public string? EmailedTo { get; init; }
}

public record InvitationUpdateDto
{
    /// <summary>Send an empty string to clear the label.</summary>
    [StringLength(100)]
    public string? Label { get; init; }

    /// <summary>Makes the invitation expire this many days from now.</summary>
    [Range(1, InvitationService.MaxExpiryDays)]
    public int? ExpiryDays { get; init; }

    /// <summary>True to make it never expire. Do not send it with <see cref="ExpiryDays"/>.</summary>
    public bool? NeverExpires { get; init; }

    /// <summary>The new limit on how many people may join, counting those who already have.</summary>
    [Range(1, InvitationService.MaxUsesLimit)]
    public int? MaxUses { get; init; }
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
    public DateTime? ExpiresAt { get; init; }
    public required InvitationStatus Status { get; init; }
    public required int UseCount { get; init; }
    public int? MaxUses { get; init; }

    /// <summary>
    /// The code, so the list can copy it. Null for invitations made before codes were kept.
    /// </summary>
    public string? Code { get; init; }
}

/// <summary>The numbers above the invitations list.</summary>
public record InvitationSummaryDto
{
    /// <summary>Invitations that can be used right now.</summary>
    public required int Active { get; init; }

    /// <summary>People who registered with any of the congregation's invitations.</summary>
    public required int PeopleJoined { get; init; }

    /// <summary>Active invitations that expire within <see cref="InvitationService.ExpiringSoonDays"/> days.</summary>
    public required int ExpiringSoon { get; init; }
}

/// <summary>What the join screen shows before the person creates an account.</summary>
public record InvitationLookupDto
{
    public required string CongregationName { get; init; }
    public required string City { get; init; }
    public required Region Region { get; init; }
    public required UserRole AllowedRole { get; init; }
    public DateTime? ExpiresAt { get; init; }
}

public record InvitationCursor
{
    public DateTime CreatedAt { get; init; }
    public Guid Id { get; init; }
}
