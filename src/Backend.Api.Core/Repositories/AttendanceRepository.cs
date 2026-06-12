using AutoMapper;
using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Common.Query;
using Backend.Api.Core.Data;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Enums;
using Backend.Api.Core.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

public class AttendanceRepository(AppDbContext context, IMapper mapper)
    : RepositoryBase<AttendanceRecord>(context, mapper)
{
    public async Task<AttendanceSummaryDto> GetSummaryAsync(
        Guid congregationId,
        DateOnly forDate,
        ChurchServiceType churchServiceType,
        CancellationToken ct = default
    )
    {
        var records = await _dbSet
            .AsNoTracking()
            .Where(a =>
                a.CongregationId == congregationId
                && a.ForDate == forDate
                && a.ChurchServiceType == churchServiceType
                && a.DeletedAt == null
            )
            .ToListAsync(ct);

        return new AttendanceSummaryDto
        {
            TotalPresent = records.Count,
            MembersPresent = records.Count(a => a.AttendeeType == AttendeeType.Member),
            FirstTimeVisitors = records.Count(a => a.AttendeeType == AttendeeType.Visitor),
            Guests = records.Count(a => a.AttendeeType == AttendeeType.Guest),
        };
    }

    public override async Task<PagedResponse<AttendanceRecord>> GetPageAsync(
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var totalRecords = await _dbSet.AsNoTracking().CountAsync(ct);

        var records = await _dbSet
            .AsNoTracking()
            .Include(a => a.Member)
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<AttendanceRecord>(records, paginationParameters, totalRecords);
    }

    public override async Task<AttendanceRecord?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Include(a => a.Member)
            .FirstOrDefaultAsync(ct);
    }
}
