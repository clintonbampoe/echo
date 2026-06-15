using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Data;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

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
