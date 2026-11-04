using Echo.Shared.Query;
using Echo.Data;
using Echo.Domain.Auth;
using Echo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Echo.Auth.EmailVerifications;

public class EmailVerificationRepository(AppDbContext context, TimeProvider timeProvider)
{
    private readonly DbSet<EmailVerificationToken> _tokens = context.Set<EmailVerificationToken>();

    public async Task<EmailVerificationToken?> GetTokenByHash(
        string hashedInput,
        CancellationToken ct
    )
    {
        var entity = await _tokens
            .FilterDeleted()
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.TokenHash == hashedInput, cancellationToken: ct);

        return entity;
    }

    /// <summary>
    /// The user with this email address and their newest verification link that can still be
    /// used, in one query. Null when there is no such user. Both are tracked.
    /// </summary>
    public async Task<(User User, EmailVerificationToken? ActiveToken)?> GetUserWithActiveToken(
        string emailAddress,
        CancellationToken ct = default
    )
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var row = await context
            .Set<User>()
            .FilterDeleted()
            .Where(u => u.EmailAddress == emailAddress)
            .Select(u => new
            {
                User = u,
                Tokens = _tokens
                    .Where(e =>
                        e.UserId == u.Id
                        && e.DeletedAt == null
                        && e.ExpiresAt > now
                        && e.InvalidatedAt == null
                        && e.UsedAt == null
                    )
                    .OrderByDescending(e => e.CreatedAt)
                    .Take(1)
                    .ToList(),
            })
            .FirstOrDefaultAsync(ct);

        return row is null ? null : (row.User, row.Tokens.FirstOrDefault());
    }

    public async Task<EmailVerificationToken?> GetTokenByUserId(
        Guid userId,
        CancellationToken ct = default
    )
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var existing = await _tokens
            .FilterDeleted()
            .Where(e =>
                e.UserId == userId
                && e.ExpiresAt > now
                && e.InvalidatedAt == null
                && e.UsedAt == null
            )
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync(ct);

        return existing;
    }

    /// <summary>
    /// Deletes the user's tokens that can never be used again: expired, used, or invalidated more
    /// than a week ago. Called whenever a new token is issued, so the table cannot grow without
    /// bound and no scheduled job is needed.
    /// </summary>
    public async Task PruneTokens(Guid userId, CancellationToken ct = default)
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

    public void Create(EmailVerificationToken emailVerificationToken)
    {
        _tokens.Add(emailVerificationToken);
    }
}
