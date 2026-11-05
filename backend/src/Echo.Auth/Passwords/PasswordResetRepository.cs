using Echo.Shared.Query;
using Echo.Data;
using Echo.Domain.Auth;
using Echo.Domain.Users;
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

    /// <summary>
    /// The user with this email address and their reset links that can still be used, in one
    /// query. Null when there is no such user. Both are tracked, so the caller can change them.
    /// </summary>
    public async Task<(User User, List<PasswordResetToken> ActiveTokens)?> GetUserWithActiveTokens(
        string emailAddress,
        CancellationToken ct
    )
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var row = await dbContext
            .Set<User>()
            .FilterDeleted()
            .Where(u => u.EmailAddress == emailAddress)
            .Select(u => new
            {
                User = u,
                Tokens = _tokens
                    .Where(t =>
                        t.UserId == u.Id
                        && t.DeletedAt == null
                        && t.ExpiresAt > now
                        && t.UsedAt == null
                        && t.InvalidatedAt == null
                    )
                    .ToList(),
            })
            .FirstOrDefaultAsync(ct);

        return row is null ? null : (row.User, row.Tokens);
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
