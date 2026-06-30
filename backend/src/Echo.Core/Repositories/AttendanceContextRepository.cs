using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Shared.Extensions.QueryMethods;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class AttendanceContextRepository(AppDbContext context)
    : ReferenceRepositoryBase<AttendanceContext>(context)
{
    public async Task<List<AttendanceContextResponseDto>> GetAllAsync(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(c => c.CongregationId == congregationId)
            .Select(c => new AttendanceContextResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                AttendanceTypeName =  c.AttendanceType.Name
            })
            .ToListAsync(ct);
    }

    public async Task<AttendanceContextResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(c => c.Id == id)
            .Select(c => new AttendanceContextResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                AttendanceTypeName =  c.AttendanceType.Name
            })
            .FirstOrDefaultAsync(ct);
    }
}
