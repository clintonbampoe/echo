using Echo.Application.Query.Extensions;
using Echo.Domain.Data;
using Echo.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace Echo.Auth.Repositories;

public class PasswordVerificationTokenRepository(AppDbContext dbContext, TimeProvider timeProvider)
{
    private readonly DbSet<PasswordVerificationToken> _tokens =
        dbContext.Set<PasswordVerificationToken>();

    public async Task<PasswordVerificationToken?> GetTokenRecordByHashWithUser(
        string hashedInput,
        CancellationToken ct
    )
    {
        return await _tokens
            .FilterSoftDeleted()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hashedInput, ct);
    }

    public async Task<PasswordVerificationToken?> GetTokenByUserId(
        Guid userId,
        CancellationToken ct
    )
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        return await _tokens
            .FilterSoftDeleted()
            .Where(t => t.UserId == userId && t.ExpiresAt > now && t.UsedAt == null)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> CreateRecord(
        PasswordVerificationToken token,
        CancellationToken ct = default
    )
    {
        await _tokens.AddAsync(token, ct);
        return true;
    }
}
