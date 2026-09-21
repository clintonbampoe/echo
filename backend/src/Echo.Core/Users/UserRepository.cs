using Echo.Application.Query;
using Echo.Data;
using Echo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Users;

public class UserRepository(AppDbContext context)
{
    private readonly DbSet<User> _dbSet = context.Set<User>();

    public async Task<List<User>> List(
        Guid congregationId,
        UserCursor? cursor,
        int pageSize,
        CancellationToken ct
    )
    {
        return await _dbSet
            .AsNoTracking()
            .FilterDeleted()
            .Where(u => u.CongregationId == congregationId)
            .OrderBy(u => u.Name)
            .ThenBy(u => u.Id)
            .Paginate(cursor, pageSize)
            .ToListAsync(ct);
    }

    public async Task<User?> GetById(Guid id, CancellationToken ct = default)
    {
        return await _dbSet.FilterDeleted().Where(u => u.Id == id).FirstOrDefaultAsync(ct);
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
    internal static IQueryable<User> Paginate(
        this IQueryable<User> query,
        UserCursor? cursor,
        int pageSize
    )
    {
        if (cursor is not null)
            query = query.Where(u =>
                string.Compare(u.Name, cursor.Name) > 0
                || (u.Name == cursor.Name && u.Id > cursor.Id)
            );

        query = query.Take(pageSize);
        return query;
    }
}
