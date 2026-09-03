using System.ComponentModel.DataAnnotations;
using Echo.Core.Dtos.Interfaces;
using Echo.Domain.Enums;

namespace Echo.Core.Dtos;

public record UserCreateDto : IPrimaryCreateDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string LastName { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string FirstName { get; init; }

    [StringLength(100)]
    public string? OtherNames { get; init; }

    [Required, EmailAddress, StringLength(255)]
    public required string EmailAddress { get; init; }

    [Required, StringLength(128, MinimumLength = 8)]
    public required string Password { get; init; }

    public UserRole Role { get; init; }
}

public record UserUpdateDto : IPrimaryUpdateDto
{
    [Required, EmailAddress, StringLength(255)]
    public required string EmailAddress { get; init; }

    [Required, StringLength(128, MinimumLength = 8)]
    public required string Password { get; init; }

    public UserRole Role { get; init; }
}

public record UserListResponseDto
    : IPrimaryListResponseDto,
        Application.Dtos.Interfaces.IPrimaryListResponseDto
{
    public Guid Id { get; init; }
    public required string EmailAddress { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime? VerifiedAt { get; init; }
    public UserRole Role { get; init; }
}

public record UserResponseDto : IPrimaryResponseDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string EmailAddress { get; init; }
    public DateTime? VerifiedAt { get; init; }
    public UserRole Role { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record UserAuthDto
{
    public Guid Id { get; init; }
    public string EmailAddress { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    public DateTime? EmailVerifiedAt { get; init; }
    public UserRole Role { get; init; }
    public Guid CongregationId { get; init; }
}
