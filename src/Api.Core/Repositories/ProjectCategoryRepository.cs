using Api.Core.Data;
using Api.Core.Dtos;
using Api.Core.Entities;
using Api.Core.Repositories.Base;
using Api.Core.Common.ExtensionMethods;
using Api.Core.Dtos.Core;
using Api.Core.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Repositories;

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
            .Select(c => new ProjectCategoryResponseDto(c.Id, c.Name))
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
            .Select(c => new ProjectCategoryResponseDto(c.Id, c.Name))
            .FirstOrDefaultAsync(ct);
    }
}
