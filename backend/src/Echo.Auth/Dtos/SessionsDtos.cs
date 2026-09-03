using System.ComponentModel.DataAnnotations;

namespace Echo.Auth.Dtos;

public record LoginRequest
{
    [Required, EmailAddress, StringLength(255)]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 1)]
    public string Password { get; init; } = string.Empty;
}

public record RefreshTokenRequest
{
    [Required, StringLength(512, MinimumLength = 1)]
    public string RefreshToken { get; init; } = string.Empty;
}
