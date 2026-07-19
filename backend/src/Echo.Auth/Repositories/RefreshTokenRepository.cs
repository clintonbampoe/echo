using Echo.Application.Extensions.QueryMethods;
using Echo.Domain.Data;
using Echo.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace Echo.Auth.Repositories;

public class RefreshTokenRepository(AppDbContext context)
{
    private readonly DbSet<RefreshToken> _tokens = context.Set<RefreshToken>();

    public async Task<RefreshToken?> GetTokenRecordByHashWithUser(string hashedInput, CancellationToken ct = default)
    {
        var tokenObject = await _tokens
            .ApplySoftDeleteFilter()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == hashedInput, ct);

        return tokenObject;
    }

    public async Task<bool> CreateNewToken(RefreshToken token, CancellationToken ct = default)
    {
        await _tokens.AddAsync(token, ct);
        return true;
    }

    public async Task<bool> Revoke(Guid tokenId, Guid? replacedByTokenId = null, CancellationToken ct = default)
    {
        var existing = await _tokens
            .ApplySoftDeleteFilter()
            .FirstOrDefaultAsync(x => x.Id == tokenId, ct);

        if (existing is null)
            return false;

        existing.RevokedAt = DateTime.UtcNow;
        existing.ReplacedByTokenId = replacedByTokenId;

        return true;
    }

    public async Task<bool> RevokeAllActiveSessionsForUser(Guid userId, CancellationToken ct = default)
    {
        var activeTokens = await _tokens
            .ApplySoftDeleteFilter()
            .Where(u => u.UserId == userId && u.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var token in activeTokens)
            token.RevokedAt = DateTime.UtcNow;

        return true;
    }
}
