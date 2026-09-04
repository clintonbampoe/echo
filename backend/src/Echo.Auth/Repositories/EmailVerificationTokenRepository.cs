using Echo.Application.Extensions.QueryMethods;
using Echo.Domain.Data;
using Echo.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace Echo.Auth.Repositories;

public class EmailVerificationTokenRepository(AppDbContext context, TimeProvider timeProvider)
{
    private readonly DbSet<EmailVerificationToken> _tokens = context.Set<EmailVerificationToken>();

    public async Task<EmailVerificationToken?> GetTokenRecordByHashWithUser(string hashedInput,
        CancellationToken ct = default)
    {
        var entity = await _tokens
            .ApplySoftDeleteFilter()
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.TokenHash == hashedInput, cancellationToken: ct);

        return entity;
    }

    public async Task<EmailVerificationToken?> GetActiveTokenForUser(Guid userId, CancellationToken ct = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var existing = await _tokens
            .ApplySoftDeleteFilter()
            .Where(e => e.UserId == userId && e.ExpiresAt > now && e.InvalidatedAt == null &&
                        e.UsedAt == null)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync(ct);

        return existing;
    }

    public async Task<bool> CreateRecord(EmailVerificationToken emailVerificationToken, CancellationToken ct = default)
    {
        await _tokens.AddAsync(emailVerificationToken, ct);
        return true;
    }

    public async Task<bool> UpdateRecord(Guid id, EmailVerificationToken token, CancellationToken ct = default)
    {
        var existing = await _tokens
            .ApplySoftDeleteFilter()
            .FirstOrDefaultAsync(e => e.Id == id, ct);
        if (existing is null)
            return false;

        _tokens.Entry(existing).CurrentValues.SetValues(token);
        _tokens.Entry(existing).Property(e => e.Id).IsModified = false;
        _tokens.Entry(existing).Property(e => e.CreatedAt).IsModified = false;
        _tokens.Entry(existing).Property(e => e.DeletedAt).IsModified = false;

        return true;
    }
}
