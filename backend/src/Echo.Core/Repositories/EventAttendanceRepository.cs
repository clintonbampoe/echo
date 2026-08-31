using Echo.Application.Extensions.QueryMethods;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
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
        var query = DbSet
            .AsNoTracking()
            .ApplyDateFilters(queryParameters)
            .Where(e => e.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .OrderBy(e => e.Id)
            .Select(e => new EventAttendanceListResponseDto
            {
                Id = e.Id,
                MemberName = e.Member.Name,
                EventName = e.Event.Name,
                CheckInTime = e.CheckInTime,
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
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await DbSet
            .AsNoTracking()
            .Where(e => e.Id == id && e.CongregationId == congregationId)
            .Select(e => new EventAttendanceResponseDto
            {
                Id = e.Id,
                MemberId = e.MemberId,
                MemberName = e.Member.Name,
                EventId = e.EventId,
                EventName = e.Event.Name,
                CheckInTime = e.CheckInTime,
                CreatedAt = e.CreatedAt,
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResponse<EventAttendanceListResponseDto>> GetByMemberId(
        PaginationParameters paginationParameters,
        QueryParameters queryParameters,
        Guid memberId,
        CancellationToken ct
    )
    {
        var query = DbSet
            .AsNoTracking()
            .ApplyDateFilters(queryParameters)
            .Where(e => e.MemberId == memberId);

        var totalCount = await query.CountAsync(ct);

        var records = await query
            .OrderBy(e => e.Id)
            .Select(e => new EventAttendanceListResponseDto
            {
                Id = e.Id,
                MemberName = e.Member.Name,
                EventName = e.Event.Name,
                CheckInTime = e.CheckInTime,
            })
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<EventAttendanceListResponseDto>(
            records,
            paginationParameters,
            totalCount
        );
    }

    public async Task<PagedResponse<EventAttendanceListResponseDto>> GetByEventId(
        PaginationParameters paginationParameters,
        QueryParameters queryParameters,
        Guid eventId,
        CancellationToken ct
    )
    {
        var query = DbSet
            .AsNoTracking()
            .ApplyDateFilters(queryParameters)
            .Where(e => e.EventId == eventId);

        var totalCount = await query.CountAsync(ct);

        var records = await query
            .OrderBy(e => e.Id)
            .Select(e => new EventAttendanceListResponseDto
            {
                Id = e.Id,
                MemberName = e.Member.Name,
                EventName = e.Event.Name,
                CheckInTime = e.CheckInTime,
            })
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<EventAttendanceListResponseDto>(
            records,
            paginationParameters,
            totalCount
        );
    }
}
