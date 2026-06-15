using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Common.Query;
using Backend.Api.Core.Data;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

public class AttendanceRepository(AppDbContext context)
    : PrimaryRepositoryBase<Attendance>(context)
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
            .Where(a => a.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .ApplyPagination(paginationParameters)
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
            .ToListAsync(ct);

        return new PagedResponse<AttendanceListResponseDto>(
            records,
            paginationParameters,
            totalRecords
        );
    }

    public async Task<AttendanceResponseDto?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default
    )
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
