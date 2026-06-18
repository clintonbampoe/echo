using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Data;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

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
            .Select(t => new AttendanceTypeResponseDto(t.Id, t.Name))
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
            .Select(t => new AttendanceTypeResponseDto(t.Id, t.Name))
            .FirstOrDefaultAsync(ct);
    }
}
