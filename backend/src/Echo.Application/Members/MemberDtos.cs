using System.ComponentModel.DataAnnotations;
using Echo.Domain.Members;
using Echo.Shared.Utilities;

namespace Echo.Application.Members;

public record MemberCreateDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string FirstName { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string LastName { get; init; }

    [StringLength(100)]
    public string? OtherNames { get; init; }

    [EmailAddress, StringLength(255)]
    public string? EmailAddress { get; init; }

    [Required, Phone, StringLength(20)]
    public required string PhoneNumber { get; init; }

    public required DateOnly DateOfBirth { get; init; }
    public required Gender Gender { get; init; }

    // Everything below is optional: it can be filled in later.
    public DateOnly? JoinedDate { get; init; }
    public DateOnly? BaptisedDate { get; init; }

    [StringLength(255)]
    public string? ResidentialAddress { get; init; }

    [StringLength(100)]
    public string? City { get; init; }

    [StringLength(100)]
    public string? Hometown { get; init; }

    public Region? Region { get; init; }

    [StringLength(255)]
    public string? GpsAddress { get; init; }

    public MaritalStatus? MaritalStatus { get; init; }

    [StringLength(100)]
    public string? NextOfKin { get; init; }

    [StringLength(100)]
    public string? EmergencyContactName { get; init; }

    [Phone, StringLength(20)]
    public string? EmergencyContactPhoneNumber { get; init; }

    public MemberStatus Status { get; init; } = MemberStatus.Active;
}

public record MemberUpdateDto
{
    [StringLength(100, MinimumLength = 1)]
    public string? FirstName { get; init; }

    [StringLength(100, MinimumLength = 1)]
    public string? LastName { get; init; }

    [StringLength(100)]
    public string? OtherNames { get; init; }

    [EmailAddress, StringLength(255)]
    public string? EmailAddress { get; init; }

    [Phone, StringLength(20)]
    public string? PhoneNumber { get; init; }

    public DateOnly? DateOfBirth { get; init; }
    public DateOnly? JoinedDate { get; init; }
    public DateOnly? BaptisedDate { get; init; }
    public Gender? Gender { get; init; }

    [StringLength(255, MinimumLength = 1)]
    public string? ResidentialAddress { get; init; }

    [StringLength(100, MinimumLength = 1)]
    public string? City { get; init; }

    [StringLength(100, MinimumLength = 1)]
    public string? Hometown { get; init; }

    public Region? Region { get; init; }

    [StringLength(255)]
    public string? GpsAddress { get; init; }

    public MaritalStatus? MaritalStatus { get; init; }

    [StringLength(100, MinimumLength = 1)]
    public string? NextOfKin { get; init; }

    [StringLength(100, MinimumLength = 1)]
    public string? EmergencyContactName { get; init; }

    [Phone, StringLength(20)]
    public string? EmergencyContactPhoneNumber { get; init; }

    public MemberStatus? Status { get; init; }
}

public record MemberResponseDto
{
    public Guid Id { get; init; } // maps to Person.Id / Member.PersonId
    public required string Name { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? OtherNames { get; init; }
    public string? EmailAddress { get; init; }
    public required string PhoneNumber { get; init; }
    public DateOnly DateOfBirth { get; init; }
    public DateOnly? JoinedDate { get; init; }
    public DateOnly? BaptisedDate { get; init; }
    public Gender Gender { get; init; }
    public string? ResidentialAddress { get; init; }
    public string? City { get; init; }
    public string? Hometown { get; init; }
    public Region? Region { get; init; }
    public string? GpsAddress { get; init; }
    public MaritalStatus? MaritalStatus { get; init; }
    public string? NextOfKin { get; init; }
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhoneNumber { get; init; }
    public MemberStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record MemberSearchResultDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string PhoneNumber { get; init; }
}

/// <summary>
/// Default window when From/To are omitted: this year (by JoinedDate).
/// Members without a JoinedDate appear in every window.
/// </summary>
public record MemberFilters
{
    public string? Name { get; init; }
    public MemberStatus? Status { get; init; }
    public Gender? Gender { get; init; }
    public Region? Region { get; init; }
    public MaritalStatus? MaritalStatus { get; init; }

    /// <summary>Only members of this organization.</summary>
    public Guid? OrganizationId { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
}

public record AgeGroupDto
{
    public required string Label { get; init; }
    public int Count { get; init; }
}

public record MemberSummaryDto
{
    public int TotalMembers { get; init; }
    public int ActiveMembers { get; init; }
    public int MaleCount { get; init; }
    public int FemaleCount { get; init; }
    public double AverageAge { get; init; }
    public IReadOnlyList<AgeGroupDto> AgeGroups { get; init; } = [];

    /// <summary>Members who have a baptised date.</summary>
    public int BaptisedMembers { get; init; }

    /// <summary>Members who joined inside the window.</summary>
    public int JoinedInWindow { get; init; }

    /// <summary>Members who joined inside the window before it: as many days, ending the day before `from`.</summary>
    public int JoinedInPreviousWindow { get; init; }

    /// <summary>Members who joined this calendar year so far, whatever the window.</summary>
    public int JoinedThisYear { get; init; }
    public DateWindow? Window { get; init; }
}

/// <summary>
/// Narrows the monthly summary. Months and To choose the months; the other filters choose the members.
/// </summary>
public record MemberMonthlyFilters
{
    public MemberStatus? Status { get; init; }
    public Gender? Gender { get; init; }

    /// <summary>How many months to return, ending with the month of <see cref="To"/>.</summary>
    [Range(1, MonthRange.MaxMonths)]
    public int Months { get; init; } = MonthRange.DefaultMonths;

    /// <summary>Any day in the last month to return. This month when not given.</summary>
    public DateOnly? To { get; init; }
}

/// <summary>The members who joined in one month.</summary>
public record MemberMonthlyDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public int NewMembers { get; init; }
}

public record MemberCursor
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
