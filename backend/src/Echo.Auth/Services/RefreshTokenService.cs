using Echo.Application.Options;
using Echo.Application.Services;
using Echo.Application.Services.Hashing;
using Echo.Auth.Dtos;
using Echo.Auth.Models;
using Echo.Auth.Repositories;
using Echo.Domain.Entities.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Echo.Auth.Services;

public class RefreshTokenService(
    RefreshTokenRepository refreshTokenRepository,
    ITokenGenerator tokenGenerator,
    [FromKeyedServices("Sha256")] IHashService tokenHashService,
    IOptions<JwtOptions> jwtOptions)
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<(RefreshToken TokenEntity, string PlainToken)> IssueAsync(Guid userId, CancellationToken ct = default)
    {
        var plainToken = tokenGenerator.GenerateToken(32);

        var tokenEntity = new RefreshToken(userId)
        {
            TokenHash = await tokenHashService.HashPasswordAsync(plainToken),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenLifetimeDays)
        };

        await refreshTokenRepository.CreateNewToken(tokenEntity, ct);

        return (tokenEntity, plainToken);
    }

    public async Task<RefreshTokenValidationResult> ValidateAndRotateAsync(string presentedToken,
        CancellationToken ct = default)
    {
        var hashedInput = await tokenHashService.HashPasswordAsync(presentedToken);
        var existing = await refreshTokenRepository.GetTokenRecordByHashWithUser(hashedInput, ct);

        if (existing is null)
            return Failure(RefreshTokenFailureReason.NotFound);

        if (existing.RevokedAt is not null)
        {
            // Already-rotated token presented again — treat as a stolen/reused token,
            // kill every active session for this user, not just this one.

            await refreshTokenRepository.RevokeAllActiveSessionsForUser(existing.UserId, ct);
            return Failure(RefreshTokenFailureReason.Reused);
        }

        if (existing.ExpiresAt <= DateTime.UtcNow)
            return Failure(RefreshTokenFailureReason.Expired);

        if (existing.User.DeletedAt is not null)
            return Failure(RefreshTokenFailureReason.UserInactive);

        var (newTokenEntity, newPlainToken) = await IssueAsync(existing.UserId, ct);

        await refreshTokenRepository.Revoke(existing.Id, newTokenEntity.Id, ct);

        return new RefreshTokenValidationResult()
        {
            Success = true,
            FailureReason = null,
            UserId = existing.UserId,
            NewRefreshToken = newPlainToken,
            NewRefreshTokenExpiresAt = newTokenEntity.ExpiresAt
        };
    }

    public async Task RevokeAsync(string presentedToken, CancellationToken ct = default)
    {
        var hashedInput = await tokenHashService.HashPasswordAsync(presentedToken);
        var existing = await refreshTokenRepository.GetTokenRecordByHashWithUser(hashedInput, ct);

        if (existing is not null)
            await refreshTokenRepository.Revoke(existing.Id, null, ct);
    }

    public async Task RevokeAllActiveSessionsForUser(Guid userId, CancellationToken ct = default)
    {
        await refreshTokenRepository.RevokeAllActiveSessionsForUser(userId, ct);
    }

    private static RefreshTokenValidationResult Failure(RefreshTokenFailureReason reason)
    {
        return new RefreshTokenValidationResult()
        {
            Success = false,
            FailureReason = reason,
            UserId = Guid.Empty,
        };
    }
}
