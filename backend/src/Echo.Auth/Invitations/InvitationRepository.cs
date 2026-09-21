using Echo.Shared.Query;
using Echo.Data;
using Echo.Domain.Auth;
using Microsoft.EntityFrameworkCore;

namespace Echo.Auth.Invitations;

public class InvitationRepository(AppDbContext context)
{
    private readonly DbSet<InvitationToken> _tokens = context.Set<InvitationToken>();

    public async Task<InvitationToken?> GetTokenRecordByHash(
        string hashedInput,
        CancellationToken ct = default
    )
    {
        return await _tokens
            .FilterDeleted()
            .FirstOrDefaultAsync(i => i.TokenHash == hashedInput, ct);
    }

    public async Task<bool> Create(InvitationToken token, CancellationToken ct = default)
    {
        await _tokens.AddAsync(token, ct);
        return true;
    }
}
