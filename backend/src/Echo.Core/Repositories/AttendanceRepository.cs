using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Shared.Extensions.QueryMethods;
using Echo.Shared.Pagination;
using Echo.Shared.Query;
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
        var query = _dbSet
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
                AttendanceContextName =  a.AttendanceContext.Name,
                AttendanceTypeName = a.AttendanceContext.AttendanceType.Name,
                MemberName =  a.Member != null ? a.Member.Name : null,
                GuestName = a.GuestName,
                AttendeeType = a.AttendeeType,
                ForDate = a.ForDate,
                CheckInTime = a.CheckInTime
            })
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
            .Select(a => new AttendanceResponseDto
            {
                Id = a.Id,
                AttendanceContextId =   a.AttendanceContext.Id,
                AttendanceContextName =  a.AttendanceContext.Name,
                AttendanceTypeName =  a.AttendanceContext.AttendanceType.Name,
                MemberName =  a.Member != null ? a.Member.Name : null,
                GuestName =  a.GuestName,
                AttendeeType =  a.AttendeeType,
                ForDate = a.ForDate,
                CheckInTime = a.CheckInTime,
                Description =   a.Description,
                CreatedAt =   a.CreatedAt
            })
            .FirstOrDefaultAsync(ct);
    }
}
