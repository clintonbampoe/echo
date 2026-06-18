using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Data;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

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
