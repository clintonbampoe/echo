using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Common.Query;
using Backend.Api.Core.Data;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

public class MemberRepository(AppDbContext context) : PrimaryRepositoryBase<Member>(context)
{
    public async Task<PagedResponse<MemberListResponseDto>> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var query = _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .ApplySearchFilter(queryParameters)
            .ApplyDateFilters(queryParameters)
            .Where(m => m.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .OrderBy(m => m.Id)
            .Select(m => new MemberListResponseDto(
                m.Id,
                m.Name,
                m.PhoneNumber,
                m.EmailAddress,
                m.Gender,
                m.MemberActivityStatus
            ))
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<MemberListResponseDto>(
            records,
            paginationParameters,
            totalRecords
        );
    }

    public async Task<MemberResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(m => m.Id == id)
            .Select(m => new MemberResponseDto(
                m.Id,
                m.Name,
                m.FirstName,
                m.LastName,
                m.OtherNames,
                m.EmailAddress,
                m.PhoneNumber,
                m.DateOfBirth,
                m.JoinedDate,
                m.Gender,
                m.ResidentialAddress,
                m.City,
                m.Hometown,
                m.Region,
                m.GpsAddress,
                m.MaritalStatus,
                m.NextOfKin,
                m.EmergencyContactName,
                m.EmergencyContactPhoneNumber,
                m.MemberActivityStatus,
                m.CreatedAt
            ))
            .FirstOrDefaultAsync(ct);
    }
}
