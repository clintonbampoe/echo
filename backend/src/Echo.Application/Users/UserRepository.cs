using Echo.Shared.Query;
using Echo.Data;
using Echo.Domain.Auth;
using Echo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Users;

public class UserRepository(AppDbContext context)
{
    private readonly DbSet<User> _dbSet = context.Set<User>();

    public async Task<List<User>> List(
        Guid congregationId,
        UserFilter filter,
        UserCursor? cursor,
        int pageSize,
        CancellationToken ct
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(u => u.CongregationId == congregationId)
            .Filter(filter)
            .OrderBy(u => u.Name)
            .ThenBy(u => u.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    /// <summary>One GROUP BY: how many people hold each role, and how many of them are disabled.</summary>
    public async Task<List<UserRoleCountDto>> CountByRole(Guid congregationId, CancellationToken ct)
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(u => u.CongregationId == congregationId)
            .GroupBy(u => u.Role)
            .Select(g => new UserRoleCountDto
            {
                Role = g.Key,
                Count = g.Count(),
                Disabled = g.Count(u => u.DisabledAt != null),
            })
            .ToListAsync(ct);
    }

    /// <summary>Admins who can still sign in, other than <paramref name="exceptId"/>.</summary>
    public async Task<bool> HasOtherActiveAdmin(
        Guid congregationId,
        Guid exceptId,
        CancellationToken ct
    )
    {
        return await _dbSet
            .FilterDeleted()
            .AnyAsync(
                u =>
                    u.CongregationId == congregationId
                    && u.Role == UserRole.Admin
                    && u.DisabledAt == null
                    && u.Id != exceptId,
                ct
            );
    }

    /// <summary>
    /// Signs the user out everywhere in one statement. An access token already issued works until it expires.
    /// </summary>
    public async Task RevokeSessions(Guid userId, DateTime now, CancellationToken ct)
    {
        await context
            .Set<RefreshToken>()
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), ct);
    }

    public async Task<User?> GetById(Guid congregationId, Guid id, CancellationToken ct = default)
    {
        return await _dbSet.FilterDeleted().ById(congregationId, id).FirstOrDefaultAsync(ct);
    }

    public async Task<List<User>> Search(Guid congregationId, string name, CancellationToken ct)
    {
        return await _dbSet
            .FilterDeleted()
            .Where(u => u.CongregationId == congregationId)
            .SearchName(name)
            .ToListAsync(ct);
    }

    public void Create(User entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(User entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }

    public async Task<User?> GetByEmail(string emailAddress, CancellationToken ct)
    {
        return await _dbSet
            .FilterDeleted()
            .Where(u => u.EmailAddress == emailAddress)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> IsEmailAddressTaken(string emailAddress, CancellationToken ct)
    {
        var exists = await _dbSet.FilterDeleted().AnyAsync(u => u.EmailAddress == emailAddress, ct);

        return exists;
    }
}

internal static class UserQueryExtensions
{
    internal static IQueryable<User> ById(
        this IQueryable<User> query,
        Guid congregationId,
        Guid id
    ) => query.Where(u => u.Id == id && u.CongregationId == congregationId);

    internal static IQueryable<User> Filter(this IQueryable<User> query, UserFilter filter)
    {
        if (filter.Role is { } role)
            query = query.Where(u => u.Role == role);

        return filter.Status switch
        {
            UserStatus.Active => query.Where(u => u.DisabledAt == null),
            UserStatus.Disabled => query.Where(u => u.DisabledAt != null),
            UserStatus.Unverified => query.Where(u => u.EmailVerifiedAt == null),
            _ => query,
        };
    }

    internal static IQueryable<User> Paginate(
        this IQueryable<User> query,
        UserCursor? cursor,
        int pageSize
    )
    {
        // The leading >= is redundant but gives the index a bound to start from; without it
        // the OR below is only a filter and a deep page walks every earlier name.
        if (cursor is not null)
            query = query.Where(u =>
                string.Compare(u.Name, cursor.Name) >= 0
                && (string.Compare(u.Name, cursor.Name) > 0 || u.Id > cursor.Id)
            );

        query = query.Take(pageSize);
        return query;
    }
}
