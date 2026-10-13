using Echo.Data;
using Echo.Domain.Auth;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;

namespace Echo.Auth.Sessions;

public class SessionRepository(AppDbContext context, TimeProvider timeProvider)
{
    private readonly DbSet<RefreshToken> _tokens = context.Set<RefreshToken>();

    public async Task<RefreshToken?> GetTokenRecordByHashWithUser(
        string hashedInput,
        CancellationToken ct = default
    )
    {
        var tokenObject = await _tokens
            .FilterDeleted()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == hashedInput, ct);

        return tokenObject;
    }

    /// <summary>
    /// Deletes the user's tokens that can never be used again: expired ones, and ones revoked
    /// more than a week ago. Called whenever a new token is issued, so the table cannot grow
    /// without bound and no scheduled job is needed.
    /// </summary>
    public async Task PruneTokens(Guid userId, CancellationToken ct = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        await _tokens.Where(t => t.UserId == userId).Prunable(now).ExecuteDeleteAsync(ct);
    }

    public async Task<bool> CreateNewToken(RefreshToken token, CancellationToken ct = default)
    {
        await _tokens.AddAsync(token, ct);
        return true;
    }

    public async Task<bool> Revoke(
        Guid tokenId,
        Guid? replacedByTokenId = null,
        CancellationToken ct = default
    )
    {
        var existing = await _tokens.FilterDeleted().FirstOrDefaultAsync(x => x.Id == tokenId, ct);

        if (existing is null)
            return false;

        existing.RevokedAt = timeProvider.GetUtcNow().UtcDateTime;
        existing.ReplacedByTokenId = replacedByTokenId;

        return true;
    }

    public async Task<bool> RevokeAllActiveSessionsForUser(
        Guid userId,
        CancellationToken ct = default
    )
    {
        var activeTokens = await _tokens
            .FilterDeleted()
            .Where(u => u.UserId == userId && u.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var token in activeTokens)
            token.RevokedAt = timeProvider.GetUtcNow().UtcDateTime;

        return true;
    }
}

internal static class RefreshTokenQueryExtensions
{
    internal const int RevokedRetentionDays = 7;

    internal static IQueryable<RefreshToken> Prunable(
        this IQueryable<RefreshToken> query,
        DateTime now
    )
    {
        var revokedBefore = now.AddDays(-RevokedRetentionDays);
        return query.Where(t =>
            t.ExpiresAt < now || (t.RevokedAt != null && t.RevokedAt < revokedBefore)
        );
    }
}
