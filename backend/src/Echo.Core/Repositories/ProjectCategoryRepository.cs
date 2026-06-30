using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Shared.Extensions.QueryMethods;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class ProjectCategoryRepository(AppDbContext context)
    : ReferenceRepositoryBase<ProjectCategory>(context)
{
    public async Task<List<ProjectCategoryResponseDto>> GetAllAsync(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(c => c.CongregationId == congregationId)
            .Select(c => new ProjectCategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToListAsync(ct);
    }

    public async Task<ProjectCategoryResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(c => c.Id == id)
            .Select(c => new ProjectCategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name
            })
            .FirstOrDefaultAsync(ct);
    }
}
