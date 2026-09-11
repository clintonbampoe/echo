using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Application.Query.Extensions;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class UserRepository(AppDbContext context)
{
    private readonly DbSet<User> _dbSet = context.Set<User>();

    public async Task<List<User>> GetPage(
        Guid congregationId,
        PaginationParameters paginationParameters,
        Parameters? queryParameters,
        CancellationToken ct
    )
    {
        var query = _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .ApplySearchFilter(queryParameters)
            .ApplyDateFilters(queryParameters)
            .Where(u => u.CongregationId == congregationId);

        var res = await query.OrderBy(u => u.Name).ThenBy(u => u.EmailAddress).ToListAsync(ct);
        return res;
    }

    public async Task<User?> GetById(Guid id, CancellationToken ct = default)
    {
        return await _dbSet.FilterSoftDeleted().Where(u => u.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task<List<User>> Search(Guid congregationId, string name, CancellationToken ct)
    {
        return await _dbSet
            .FilterSoftDeleted()
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
            .FilterSoftDeleted()
            .Where(u => u.EmailAddress == emailAddress)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> IsEmailAddressTaken(string emailAddress, CancellationToken ct)
    {
        var exists = await _dbSet
            .FilterSoftDeleted()
            .AnyAsync(u => u.EmailAddress == emailAddress, ct);

        return exists;
    }
}
