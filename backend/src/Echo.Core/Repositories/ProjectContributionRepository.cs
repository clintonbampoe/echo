using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Application.Query.Extensions;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class ProjectContributionRepository(AppDbContext context)
{
    private readonly DbSet<ProjectContribution> _dbSet = context.Set<ProjectContribution>();

    public async Task<List<ProjectContribution>> GetPage(
        Guid congregationId,
        PaginationParameters paginationParameters,
        Parameters? queryParameters,
        CancellationToken ct
    )
    {
        var query = _dbSet
            .AsNoTracking()
            .FilterSoftDeleted()
            .ApplyDateFilters(queryParameters)
            .Where(p => p.CongregationId == congregationId);

        var res = await query.OrderBy(p => p.Id).Include(p => p.Project).ToListAsync(ct);
        return res;
    }

    public async Task<ProjectContribution?> GetById(
        Guid congregationId,
        Guid id,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .FilterSoftDeleted()
            .Where(p => p.Id == id && p.CongregationId == congregationId)
            .Include(p => p.Project)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<ProjectContribution>> GetByProjectId(
        Guid congregationId,
        Guid projectId,
        CancellationToken ct
    )
    {
        return await _dbSet
            .FilterSoftDeleted()
            .AsNoTracking()
            .Where(p => p.CongregationId == congregationId && p.ProjectId == projectId)
            .Include(p => p.Project)
            .ToListAsync(ct);
    }

    public void Create(ProjectContribution entity)
    {
        _dbSet.Add(entity);
    }

    public void SoftDelete(ProjectContribution entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
    }

    public Task GetSummary(Guid congregationId, Guid projectId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
