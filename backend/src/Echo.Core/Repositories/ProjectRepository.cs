using Echo.Application.Extensions.QueryExtensions;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class ProjectRepository(AppDbContext context)
{
    private readonly DbSet<Project> _dbSet = context.Set<Project>();

    public async Task<List<Project>> GetPage(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct
    )
    {
        var query = _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .ApplySearchFilter(queryParameters)
            .ApplyDateFilters(queryParameters)
            .Where(p => p.CongregationId == congregationId);

        var res = await query
            .OrderBy(p => p.Id)
            .Include(p => p.Category)
            .Include(p => p.Manager)
            .ToListAsync(ct);

        return res;
    }

    public async Task<Project?> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(p => p.Id == id && p.CongregationId == congregationId)
            .Include(p => p.Category)
            .Include(p => p.Manager)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<Project>> Search(Guid congregationId, string name, CancellationToken ct)
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(pr => pr.CongregationId == congregationId)
            .SearchName(name)
            .ToListAsync(ct);
    }

    public void Create(Project entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(Project entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }

    public Task GetSummary(Guid congregationId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
