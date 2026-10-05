using System.ComponentModel.DataAnnotations;

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

    public DateOnly? DateOfBirth { get; init; }

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

    public DateOnly? DateOfBirth { get; init; }

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
    public DateOnly? DateOfBirth { get; init; }
    public string? Notes { get; init; }
    public Guid? ConvertedToMemberPersonId { get; init; }
    public string? ConvertedToMemberName { get; init; }
    public DateTime? ConvertedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record VisitorSearchResultDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? PhoneNumber { get; init; }
}

public record VisitorFilters
{
    public string? Name { get; init; }
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
}

public record VisitorCursor
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
