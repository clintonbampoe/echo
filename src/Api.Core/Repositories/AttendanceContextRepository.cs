using Api.Core.Data;
using Api.Core.Dtos;
using Api.Core.Entities;
using Api.Core.Repositories.Base;
using Api.Core.Common.ExtensionMethods;
using Api.Core.Dtos.Core;
using Api.Core.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Repositories;

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
            .Select(c => new AttendanceContextResponseDto(c.Id, c.Name, c.AttendanceType.Name))
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
            .Select(c => new AttendanceContextResponseDto(c.Id, c.Name, c.AttendanceType.Name))
            .FirstOrDefaultAsync(ct);
    }
}
