using Echo.Domain.Auth;

namespace Echo.Auth.Invitations;

internal static class InvitationQueryExtensions
{
    // Revoked wins over expired, and expired over used up; an invitation expires at the instant its
    // ExpiresAt is reached.
    internal static InvitationStatus StatusAt(this InvitationToken token, DateTime now) =>
        token.IsRevoked ? InvitationStatus.Revoked
        : token.ExpiresAt <= now ? InvitationStatus.Expired
        : token.MaxUses <= token.UseCount ? InvitationStatus.Exhausted
        : InvitationStatus.Active;

    internal static IQueryable<InvitationToken> WithStatus(
        this IQueryable<InvitationToken> query,
        InvitationStatus? status,
        DateTime now
    ) =>
        status switch
        {
            InvitationStatus.Revoked => query.Where(t => t.IsRevoked),
            InvitationStatus.Expired => query.Where(t => !t.IsRevoked && t.ExpiresAt <= now),
            InvitationStatus.Exhausted => query.Where(t =>
                !t.IsRevoked
                && (t.ExpiresAt == null || t.ExpiresAt > now)
                && t.MaxUses != null
                && t.MaxUses <= t.UseCount
            ),
            InvitationStatus.Active => query.Where(t =>
                !t.IsRevoked
                && (t.ExpiresAt == null || t.ExpiresAt > now)
                && (t.MaxUses == null || t.MaxUses > t.UseCount)
            ),
            _ => query,
        };

    // The list is ordered newest first, then by id; the cursor is the last row of the previous page.
    internal static IQueryable<InvitationToken> Paginate(
        this IQueryable<InvitationToken> query,
        InvitationCursor? cursor,
        int pageSize
    )
    {
        if (cursor is not null)
            query = query.Where(t =>
                t.CreatedAt <= cursor.CreatedAt
                && (t.CreatedAt < cursor.CreatedAt || t.Id > cursor.Id)
            );

        return query.Take(pageSize);
    }
}
