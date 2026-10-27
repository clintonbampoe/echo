using System.ComponentModel.DataAnnotations;
using Echo.Domain.Members;
using Echo.Shared.Utilities;

namespace Echo.Application.Members;

public record VisitorCreateDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string FirstName { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string LastName { get; init; }

    [Phone, StringLength(20)]
    public string? PhoneNumber { get; init; }

    [EmailAddress, StringLength(255)]
    public string? EmailAddress { get; init; }

    public required Gender Gender { get; init; }

    [StringLength(255)]
    public string? Address { get; init; }

    public VisitorSource? Source { get; init; }
    public Guid? InvitedByMemberId { get; init; }

    [StringLength(2000)]
    public string? Notes { get; init; }
}

public record VisitorUpdateDto
{
    [StringLength(100, MinimumLength = 1)]
    public string? FirstName { get; init; }

    [StringLength(100, MinimumLength = 1)]
    public string? LastName { get; init; }

    [Phone, StringLength(20)]
    public string? PhoneNumber { get; init; }

    [EmailAddress, StringLength(255)]
    public string? EmailAddress { get; init; }

    public Gender? Gender { get; init; }

    [StringLength(255)]
    public string? Address { get; init; }

    public VisitorSource? Source { get; init; }
    public Guid? InvitedByMemberId { get; init; }

    [StringLength(2000)]
    public string? Notes { get; init; }
}

public record VisitorResponseDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? PhoneNumber { get; init; }
    public string? EmailAddress { get; init; }

    /// <summary>Male or Female. Empty for visitors recorded before gender was asked.</summary>
    public Gender? Gender { get; init; }
    public string? Address { get; init; }

    /// <summary>How they came: `WalkIn`, `Invited`, `Online` or `Other`. Empty for visitors recorded before it was asked.</summary>
    public VisitorSource? Source { get; init; }

    /// <summary>The member who invited them, when known.</summary>
    public Guid? InvitedByMemberId { get; init; }
    public string? InvitedByName { get; init; }
    public string? Notes { get; init; }
    public Guid? ConvertedToMemberPersonId { get; init; }
    public string? ConvertedToMemberName { get; init; }
    public DateTime? ConvertedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public int VisitCount { get; init; }
    public DateOnly? LastVisitedAt { get; init; }
}

public record VisitorSearchResultDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? PhoneNumber { get; init; }
}

/// <summary>
/// Default window when From/To are omitted: this year. The window applies to CreatedAt
/// for the current (unconverted) view and to ConvertedAt when Converted = true.
/// </summary>
public record VisitorFilters
{
    public string? Name { get; init; }
    public Gender? Gender { get; init; }
    public VisitorSource? Source { get; init; }
    public Guid? InvitedByMemberId { get; init; }
    public bool? Converted { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; } // on CreatedAt
}

public record VisitorSummaryDto
{
    public int TotalVisitors { get; init; }
    public int NewVisitors { get; init; }
    public int RecurringVisitors { get; init; }
    public int ConvertedVisitors { get; init; }

    /// <summary>Male visitors in the visitor list. Visitors with no gender recorded are in neither count.</summary>
    public int InvitedVisitors { get; init; }
    public int MaleVisitors { get; init; }

    /// <summary>Female visitors in the visitor list.</summary>
    public int FemaleVisitors { get; init; }
    public DateWindow? Window { get; init; }
}

public record VisitorCursor
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
