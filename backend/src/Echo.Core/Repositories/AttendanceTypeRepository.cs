using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
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
        return await DbSet
            .AsNoTracking()
            .Where(t => t.CongregationId == congregationId)
            .Select(t => new AttendanceTypeResponseDto { Id = t.Id, Name = t.Name })
            .ToListAsync(ct);
    }

    public async Task<AttendanceTypeResponseDto?> GetByIdAsync(
        int id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await DbSet
            .AsNoTracking()
            .Where(t => t.Id == id && t.CongregationId == congregationId)
            .Select(t => new AttendanceTypeResponseDto { Id = t.Id, Name = t.Name })
            .FirstOrDefaultAsync(ct);
    }
}
