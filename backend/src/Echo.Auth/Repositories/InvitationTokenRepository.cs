using Echo.Application.Extensions.QueryMethods;
using Echo.Domain.Data;
using Echo.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace Echo.Auth.Repositories;

public class InvitationTokenRepository(AppDbContext context)
{
    private readonly DbSet<InvitationToken> _tokens = context.Set<InvitationToken>();

    public async Task<InvitationToken?> GetTokenRecordByHash(string hashedInput, CancellationToken ct = default)
    {
        return await _tokens
            .ApplySoftDeleteFilter()
            .FirstOrDefaultAsync(i => i.TokenHash == hashedInput, ct);
    }

    public async Task<bool> CreateRecord(InvitationToken token, CancellationToken ct = default)
    {
        await _tokens.AddAsync(token, ct);
        return true;
    }
}
