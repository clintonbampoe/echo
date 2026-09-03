using System.ComponentModel.DataAnnotations;

namespace Echo.Auth.Dtos;

public record ForgotPasswordRequest
{
    [Required, EmailAddress, StringLength(255)]
    public string Email { get; init; } = string.Empty;
}

public record ResetPasswordRequest
{
    [Required, EmailAddress, StringLength(255)]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 8)]
    public string NewPassword { get; init; } = string.Empty;

    [Required, StringLength(512, MinimumLength = 1)]
    public string Token { get; init; } = string.Empty;
}
