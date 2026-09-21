using Echo.Application.Query;
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

    public async Task<PasswordResetToken?> GetTokenByUserId(Guid userId, CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        return await _tokens
            .FilterDeleted()
            .Where(t => t.UserId == userId && t.ExpiresAt > now && t.UsedAt == null)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> CreateRecord(PasswordResetToken token, CancellationToken ct = default)
    {
        await _tokens.AddAsync(token, ct);
        return true;
    }
}
