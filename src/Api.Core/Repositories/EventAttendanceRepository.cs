using Api.Core.Common.Pagination;
using Api.Core.Common.Query;
using Api.Core.Data;
using Api.Core.Dtos;
using Api.Core.Entities;
using Api.Core.Repositories.Base;
using Api.Core.Common.ExtensionMethods;
using Api.Core.Dtos.Core;
using Api.Core.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Repositories;

public class EventAttendanceRepository(AppDbContext context)
    : PrimaryRepositoryBase<EventAttendance>(context)
{
    public async Task<PagedResponse<EventAttendanceListResponseDto>> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var query = _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .ApplyDateFilters(queryParameters)
            .Where(e => e.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .OrderBy(e => e.Id)
            .Select(e => new EventAttendanceListResponseDto(
                e.Id,
                e.Member.Name,
                e.Event.Name,
                e.CheckInTime
            ))
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<EventAttendanceListResponseDto>(
            records,
            paginationParameters,
            totalRecords
        );
    }

    public async Task<EventAttendanceResponseDto?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(e => e.Id == id)
            .Select(e => new EventAttendanceResponseDto(
                e.Id,
                e.MemberId,
                e.Member.Name,
                e.EventId,
                e.Event.Name,
                e.CheckInTime,
                e.CreatedAt
            ))
            .FirstOrDefaultAsync(ct);
    }
}
