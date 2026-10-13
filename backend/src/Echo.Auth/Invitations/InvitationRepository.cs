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

    public async Task<InvitationToken?> GetTokenRecordByHashWithCongregation(
        string hashedInput,
        CancellationToken ct = default
    )
    {
        return await _tokens
            .AsNoTracking()
            .FilterDeleted()
            .Include(i => i.Congregation)
            .FirstOrDefaultAsync(i => i.TokenHash == hashedInput, ct);
    }

    public async Task<InvitationToken?> GetById(
        Guid congregationId,
        Guid id,
        CancellationToken ct = default
    )
    {
        return await _tokens
            .FilterDeleted()
            .Include(i => i.CreatedBy)
            .FirstOrDefaultAsync(i => i.Id == id && i.CongregationId == congregationId, ct);
    }

    public async Task<List<InvitationToken>> List(
        Guid congregationId,
        InvitationStatus? status,
        InvitationCursor? cursor,
        int pageSize,
        DateTime now,
        CancellationToken ct = default
    )
    {
        return await _tokens
            .AsNoTracking()
            .FilterDeleted()
            .Where(i => i.CongregationId == congregationId)
            .WithStatus(status, now)
            .Include(i => i.CreatedBy)
            .OrderByDescending(i => i.CreatedAt)
            .ThenBy(i => i.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    // One atomic increment, so two people registering at once are both counted.
    public async Task RecordUse(Guid id, CancellationToken ct = default)
    {
        await _tokens
            .Where(i => i.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.UseCount, i => i.UseCount + 1), ct);
    }

    public void SoftDelete(InvitationToken token) => token.DeletedAt = DateTime.UtcNow;

    public async Task<bool> Create(InvitationToken token, CancellationToken ct = default)
    {
        await _tokens.AddAsync(token, ct);
        return true;
    }
}
