using Echo.Auth.Models;

namespace Echo.Auth.Dtos;

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
    public RefreshTokenFailureReason? FailureReason { get; init; }
    public Guid UserId { get; init; }
    public string? NewRefreshToken { get; init; }
    public DateTime? NewRefreshTokenExpiresAt { get; init; }
}
