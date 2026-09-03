using System.ComponentModel.DataAnnotations;
using Echo.Core.Dtos.Interfaces;
using Echo.Domain.Enums;

namespace Echo.Core.Dtos;

public record MemberCreateDto : IPrimaryCreateDto
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

    public DateOnly DateOfBirth { get; init; }
    public DateOnly? JoinedDate { get; init; }
    public Gender Gender { get; init; }

    [Required, StringLength(255, MinimumLength = 1)]
    public required string ResidentialAddress { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string City { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string Hometown { get; init; }

    public Region Region { get; init; }

    [StringLength(255)]
    public string? GpsAddress { get; init; }

    public MaritalStatus MaritalStatus { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string NextOfKin { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string EmergencyContactName { get; init; }

    [Required, Phone, StringLength(20)]
    public required string EmergencyContactPhoneNumber { get; init; }

    public MemberActivityStatus MemberActivityStatus { get; init; }
}

public record MemberUpdateDto : IPrimaryUpdateDto
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

    public DateOnly DateOfBirth { get; init; }
    public DateOnly? JoinedDate { get; init; }
    public Gender Gender { get; init; }

    [Required, StringLength(255, MinimumLength = 1)]
    public required string ResidentialAddress { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string City { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string Hometown { get; init; }

    public Region Region { get; init; }

    [StringLength(255)]
    public string? GpsAddress { get; init; }

    public MaritalStatus MaritalStatus { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string NextOfKin { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string EmergencyContactName { get; init; }

    [Required, Phone, StringLength(20)]
    public required string EmergencyContactPhoneNumber { get; init; }

    public MemberActivityStatus MemberActivityStatus { get; init; }
}

public record MemberListResponseDto : IPrimaryListResponseDto, Application.Dtos.Interfaces.IPrimaryListResponseDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string PhoneNumber { get; init; }
    public string? EmailAddress { get; init; }
    public Gender Gender { get; init; }
    public MemberActivityStatus MemberActivityStatus { get; init; }
}

public record MemberResponseDto : IPrimaryResponseDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? OtherNames { get; init; }
    public string? EmailAddress { get; init; }
    public required string PhoneNumber { get; init; }
    public DateOnly DateOfBirth { get; init; }
    public DateOnly? JoinedDate { get; init; }
    public Gender Gender { get; init; }
    public required string ResidentialAddress { get; init; }
    public required string City { get; init; }
    public required string Hometown { get; init; }
    public Region Region { get; init; }
    public string? GpsAddress { get; init; }
    public MaritalStatus MaritalStatus { get; init; }
    public required string NextOfKin { get; init; }
    public required string EmergencyContactName { get; init; }
    public required string EmergencyContactPhoneNumber { get; init; }
    public MemberActivityStatus MemberActivityStatus { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record MemberSummaryDto
{
    public required int TotalMembership { get; init; }
    public required int NewMembers { get; init; }
    public required decimal RetentionRate { get; init; }
}
