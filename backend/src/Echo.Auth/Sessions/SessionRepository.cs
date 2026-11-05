using Echo.Data;
using Echo.Domain.Auth;
using Echo.Domain.Users;
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

    /// <summary>Marks an already loaded token revoked; saved by the caller's CommitAsync.</summary>
    public void Revoke(RefreshToken token, Guid? replacedByTokenId = null)
    {
        token.RevokedAt = timeProvider.GetUtcNow().UtcDateTime;
        token.ReplacedByTokenId = replacedByTokenId;
    }

    /// <summary>
    /// Revokes the token with this hash in one statement, if it is still active. A token that is
    /// unknown or already revoked is left alone, so its original revoke time is kept.
    /// </summary>
    public async Task RevokeByHash(string hashedInput, CancellationToken ct = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        await _tokens
            .FilterDeleted()
            .Where(t => t.TokenHash == hashedInput && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), ct);
    }

    /// <summary>
    /// Revokes every active token of the account with this email address in one statement, so
    /// the user does not have to be loaded first. Returns how many tokens it revoked.
    /// </summary>
    public async Task<int> RevokeAllActiveSessionsForEmail(
        string emailAddress,
        CancellationToken ct = default
    )
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        return await _tokens
            .FilterDeleted()
            .Where(t =>
                t.RevokedAt == null
                && context
                    .Set<User>()
                    .Any(u => u.Id == t.UserId && u.EmailAddress == emailAddress && u.DeletedAt == null)
            )
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), ct);
    }

    /// <summary>
    /// Revokes all of the user's active tokens in one statement. It runs immediately, not on
    /// CommitAsync: when it belongs with other changes (a password change), call it inside
    /// <see cref="IUnitOfWork.ExecuteInTransactionAsync"/>.
    /// </summary>
    public async Task RevokeAllActiveSessionsForUser(Guid userId, CancellationToken ct = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        await _tokens
            .FilterDeleted()
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), ct);
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
