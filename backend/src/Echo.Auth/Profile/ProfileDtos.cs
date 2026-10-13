using System.ComponentModel.DataAnnotations;

namespace Echo.Auth.Profile;

/// <summary>
/// What a signed-in user may change about themselves. Only the fields that are sent change.
/// The role and email address are not here on purpose.
/// </summary>
public record ProfileUpdateDto
{
    [StringLength(100)]
    public string? FirstName { get; init; }

    [StringLength(100)]
    public string? LastName { get; init; }

    [StringLength(100)]
    public string? OtherNames { get; init; }

    [StringLength(PasswordPolicy.MaxLength)]
    public string? CurrentPassword { get; init; }

    [StringLength(PasswordPolicy.MaxLength)]
    public string? NewPassword { get; init; }
}
