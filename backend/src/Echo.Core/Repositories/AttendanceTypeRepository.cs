using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Shared.Extensions.QueryMethods;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class AttendanceTypeRepository(AppDbContext context)
    : ReferenceRepositoryBase<AttendanceType>(context)
{
    public async Task<List<AttendanceTypeResponseDto>> GetAllAsync(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(t => t.CongregationId == congregationId)
            .Select(t => new AttendanceTypeResponseDto
            {
                Id = t.Id,
                Name = t.Name
            })
            .ToListAsync(ct);
    }

    public async Task<AttendanceTypeResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(t => t.Id == id)
            .Select(t => new AttendanceTypeResponseDto
            {
                Id = t.Id,
                Name = t.Name
            })
            .FirstOrDefaultAsync(ct);
    }
}
