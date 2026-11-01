using Echo.Domain.Users;

namespace Echo.Auth.Sessions;

public record TokenPairResponseDtos
{
    public string AccessToken { get; init; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; init; }

    public string RefreshToken { get; init; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; init; }
}

public record RefreshTokenValidationResult
{
    public bool Success { get; init; }
    public RefreshSessionFailure? FailureReason { get; init; }
    public Guid UserId { get; init; }

    /// <summary>The token's user, already loaded; set when <see cref="Success"/> is true.</summary>
    public User? User { get; init; }
    public string? NewRefreshToken { get; init; }
    public DateTime? NewRefreshTokenExpiresAt { get; init; }
}
