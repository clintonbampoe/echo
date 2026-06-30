using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Shared.Extensions.QueryMethods;
using Echo.Shared.Pagination;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

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
            .Select(e => new EventAttendanceListResponseDto
            {
                Id = e.Id,
                MemberName =  e.Member.Name,
                EventName =  e.Event.Name,
                CheckInTime = e.CheckInTime
            })
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
            .Select(e => new EventAttendanceResponseDto
            {
                Id = e.Id,
                MemberId =  e.MemberId,
                MemberName = e.Member.Name,
                EventId =  e.EventId,
                EventName = e.Event.Name,
                CheckInTime = e.CheckInTime,
                CreatedAt =  e.CreatedAt
            })
            .FirstOrDefaultAsync(ct);
    }
}
