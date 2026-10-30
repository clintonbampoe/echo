using Echo.Shared.Query;
using Echo.Data;
using Echo.Domain.Auth;
using Microsoft.EntityFrameworkCore;

namespace Echo.Auth.Invitations;

public class InvitationRepository(AppDbContext context, TimeProvider timeProvider)
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

    // Takes one of the invitation's uses, in one statement: of two people registering at once when
    // one use is left, only one gets it. False when it is revoked, expired or used up.
    public async Task<bool> TryClaimUse(Guid id, DateTime now, CancellationToken ct = default)
    {
        var claimed = await _tokens
            .Where(i =>
                i.Id == id
                && !i.IsRevoked
                && (i.ExpiresAt == null || i.ExpiresAt > now)
                && (i.MaxUses == null || i.MaxUses > i.UseCount)
            )
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.UseCount, i => i.UseCount + 1), ct);
        return claimed > 0;
    }

    // Counts for the stat strip, in one pass over the congregation's invitations.
    public async Task<InvitationSummaryDto> GetSummary(
        Guid congregationId,
        DateTime now,
        DateTime soon,
        CancellationToken ct = default
    )
    {
        var row = await _tokens
            .AsNoTracking()
            .FilterDeleted()
            .Where(i => i.CongregationId == congregationId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Active = g.Count(i =>
                    !i.IsRevoked
                    && (i.ExpiresAt == null || i.ExpiresAt > now)
                    && (i.MaxUses == null || i.MaxUses > i.UseCount)
                ),
                PeopleJoined = g.Sum(i => i.UseCount),
                ExpiringSoon = g.Count(i =>
                    !i.IsRevoked
                    && i.ExpiresAt > now
                    && i.ExpiresAt <= soon
                    && (i.MaxUses == null || i.MaxUses > i.UseCount)
                ),
            })
            .FirstOrDefaultAsync(ct);

        return new InvitationSummaryDto
        {
            Active = row?.Active ?? 0,
            PeopleJoined = row?.PeopleJoined ?? 0,
            ExpiringSoon = row?.ExpiringSoon ?? 0,
        };
    }

    public void SoftDelete(InvitationToken token) =>
        token.DeletedAt = timeProvider.GetUtcNow().UtcDateTime;

    public async Task<bool> Create(InvitationToken token, CancellationToken ct = default)
    {
        await _tokens.AddAsync(token, ct);
        return true;
    }
}
