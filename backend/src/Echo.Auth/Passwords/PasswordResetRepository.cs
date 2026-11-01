using Echo.Shared.Query;
using Echo.Data;
using Echo.Domain.Auth;
using Microsoft.EntityFrameworkCore;

namespace Echo.Auth.Passwords;

public class PasswordResetRepository(AppDbContext dbContext, TimeProvider timeProvider)
{
    private readonly DbSet<PasswordResetToken> _tokens = dbContext.Set<PasswordResetToken>();

    public async Task<PasswordResetToken?> GetTokenRecordByHashWithUser(
        string hashedInput,
        CancellationToken ct
    )
    {
        return await _tokens
            .FilterDeleted()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hashedInput, ct);
    }

    /// <summary>The user's reset links that can still be used.</summary>
    public async Task<List<PasswordResetToken>> GetActiveTokensForUser(
        Guid userId,
        CancellationToken ct
    )
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        return await _tokens
            .FilterDeleted()
            .Where(t =>
                t.UserId == userId
                && t.ExpiresAt > now
                && t.UsedAt == null
                && t.InvalidatedAt == null
            )
            .ToListAsync(ct);
    }

    /// <summary>
    /// Deletes the user's tokens that can never be used again: expired, used, or invalidated more
    /// than a week ago. Called whenever a new token is issued, so the table cannot grow without
    /// bound and no scheduled job is needed.
    /// </summary>
    public async Task PruneTokens(Guid userId, CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var invalidatedBefore = now.AddDays(-7);
        await _tokens
            .Where(t =>
                t.UserId == userId
                && (
                    t.ExpiresAt < now
                    || t.UsedAt != null
                    || (t.InvalidatedAt != null && t.InvalidatedAt < invalidatedBefore)
                )
            )
            .ExecuteDeleteAsync(ct);
    }

    public async Task<bool> CreateRecord(PasswordResetToken token, CancellationToken ct = default)
    {
        await _tokens.AddAsync(token, ct);
        return true;
    }
}
