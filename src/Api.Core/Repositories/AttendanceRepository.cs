using Api.Core.Common.Pagination;
using Api.Core.Common.Query;
using Api.Core.Data;
using Api.Core.Dtos;
using Api.Core.Entities;
using Api.Core.Repositories.Base;
using Api.Core.Common.ExtensionMethods;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Repositories;

public class AttendanceRepository(AppDbContext context) : PrimaryRepositoryBase<Attendance>(context)
{
    public async Task<PagedResponse<AttendanceListResponseDto>> GetPageAsync(
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
            .Where(a => a.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .OrderBy(a => a.Id)
            .Select(a => new AttendanceListResponseDto(
                a.Id,
                a.AttendanceContext.Name,
                a.AttendanceContext.AttendanceType.Name,
                a.Member != null ? a.Member.Name : null,
                a.GuestName,
                a.AttendeeType,
                a.ForDate,
                a.CheckInTime
            ))
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<AttendanceListResponseDto>(
            records,
            paginationParameters,
            totalRecords
        );
    }

    public async Task<AttendanceResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(a => a.Id == id)
            .Select(a => new AttendanceResponseDto(
                a.Id,
                a.AttendanceContextId,
                a.AttendanceContext.Name,
                a.AttendanceContext.AttendanceType.Name,
                a.MemberId,
                a.Member != null ? a.Member.Name : null,
                a.GuestName,
                a.AttendeeType,
                a.ForDate,
                a.CheckInTime,
                a.Description,
                a.CreatedAt
            ))
            .FirstOrDefaultAsync(ct);
    }
}
