using Echo.Domain.Data;
using Echo.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace Echo.Auth.Repositories;

public class PasswordVerificationTokenRepository(AppDbContext dbContext)
{
    private readonly DbSet<PasswordVerificationToken> _tokens = dbContext.Set<PasswordVerificationToken>();

    public async Task<PasswordVerificationToken?> GetTokenRecordByHashWithUser(string hashedInput,
        CancellationToken ct = default)
    {
        return await _tokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hashedInput, ct);
    }

    public async Task<PasswordVerificationToken?> GetActiveTokenForUser(Guid userId, CancellationToken ct = default)
    {
        return await _tokens
            .Where(t => t.UserId == userId && t.ExpiresAt > DateTime.UtcNow && t.UsedAt == null)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> CreateRecord(PasswordVerificationToken token, CancellationToken ct = default)
    {
        await _tokens.AddAsync(token, ct);
        return true;
    }
}
