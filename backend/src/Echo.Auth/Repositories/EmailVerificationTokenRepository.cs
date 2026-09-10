using Echo.Application.Extensions.QueryExtensions;
using Echo.Domain.Data;
using Echo.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace Echo.Auth.Repositories;

public class EmailVerificationTokenRepository(AppDbContext context, TimeProvider timeProvider)
{
    private readonly DbSet<EmailVerificationToken> _tokens = context.Set<EmailVerificationToken>();

    public async Task<EmailVerificationToken?> GetTokenByHash(
        string hashedInput,
        CancellationToken ct
    )
    {
        var entity = await _tokens
            .FilterSoftDeleted()
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.TokenHash == hashedInput, cancellationToken: ct);

        return entity;
    }

    public async Task<EmailVerificationToken?> GetTokenByUserId(
        Guid userId,
        CancellationToken ct = default
    )
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var existing = await _tokens
            .FilterSoftDeleted()
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

    public void Create(EmailVerificationToken emailVerificationToken)
    {
        _tokens.Add(emailVerificationToken);
    }
}
