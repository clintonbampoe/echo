using Echo.Application.Extensions.QueryMethods;
using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
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
        return await DbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(c => c.CongregationId == congregationId)
            .Select(c => new AttendanceContextResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                AttendanceTypeName = c.AttendanceType.Name,
            })
            .ToListAsync(ct);
    }

    public async Task<AttendanceContextResponseDto?> GetByIdAsync(
        int id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await DbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(c => c.Id == id && c.CongregationId == congregationId)
            .Select(c => new AttendanceContextResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                AttendanceTypeName = c.AttendanceType.Name,
            })
            .FirstOrDefaultAsync(ct);
    }
}
