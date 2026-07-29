using Echo.Application.Extensions.QueryMethods;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class AttendanceRepository(AppDbContext context) : PrimaryRepositoryBase<Attendance>(context)
{
    public async Task<PagedResponse<AttendanceListResponseDto>> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var query = DbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .ApplyDateFilters(queryParameters)
            .Where(a => a.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .OrderBy(a => a.Id)
            .Select(a => new AttendanceListResponseDto
            {
                Id = a.Id,
                AttendanceContextName = a.AttendanceContext.Name,
                AttendanceTypeName = a.AttendanceContext.AttendanceType.Name,
                MemberName = a.Member != null ? a.Member.Name : null,
                GuestName = a.GuestName,
                AttendeeType = a.AttendeeType,
                ForDate = a.ForDate,
                CheckInTime = a.CheckInTime,
            })
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<AttendanceListResponseDto>(
            records,
            paginationParameters,
            totalRecords
        );
    }

    public async Task<AttendanceResponseDto?> GetByIdAsync(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await DbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(a => a.Id == id && a.CongregationId == congregationId)
            .Select(a => new AttendanceResponseDto
            {
                Id = a.Id,
                AttendanceContextId = a.AttendanceContext.Id,
                AttendanceContextName = a.AttendanceContext.Name,
                AttendanceTypeName = a.AttendanceContext.AttendanceType.Name,
                MemberName = a.Member != null ? a.Member.Name : null,
                GuestName = a.GuestName,
                AttendeeType = a.AttendeeType,
                ForDate = a.ForDate,
                CheckInTime = a.CheckInTime,
                Description = a.Description,
                CreatedAt = a.CreatedAt,
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<AttendanceSummaryDto> GetSummaryAsync(
        Guid congregationId,
        int attendanceContextId,
        DateOnly forDate,
        CancellationToken ct = default
    )
    {
        var attendeesForToday = await DbSet
            .ApplySoftDeleteFilter()
            .Where(a =>
                a.CongregationId == congregationId
                && a.AttendanceContextId == attendanceContextId
                && a.ForDate == forDate
            )
            .Select(a => new { a.AttendeeType, a.GuestName })
            .ToListAsync(ct);

        var membersPresent = attendeesForToday.Count(a => a.AttendeeType == AttendeeType.Member);
        var children = attendeesForToday.Count(a => a.AttendeeType == AttendeeType.Child);

        var guestNames = attendeesForToday
            .Where(a =>
                a.AttendeeType is AttendeeType.Guest or AttendeeType.Visitor && a.GuestName != null
            )
            .Select(a => a.GuestName)
            .Distinct()
            .ToList();

        var returningGuestNames =
            guestNames.Count == 0
                ? []
                : await DbSet
                    .ApplySoftDeleteFilter()
                    .Where(a =>
                        a.CongregationId == congregationId
                        && a.ForDate < forDate
                        && a.GuestName != null
                        && guestNames.Contains(a.GuestName)
                    )
                    .Select(a => a.GuestName)
                    .Distinct()
                    .ToListAsync(ct);

        return new AttendanceSummaryDto
        {
            TotalPresent = attendeesForToday.Count,
            FirstTimeVisitors = guestNames.Except(returningGuestNames).Count(),
            MembersPresent = membersPresent,
            Children = children,
        };
    }
}
