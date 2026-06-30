using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Shared.Extensions.QueryMethods;
using Echo.Shared.Pagination;
using Echo.Shared.Query;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

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
            .Select(m => new MemberListResponseDto
            {
                Id = m.Id,
                Name = m.Name,
                PhoneNumber =  m.PhoneNumber,
                EmailAddress = m.EmailAddress,
                Gender = m.Gender,
                MemberActivityStatus = m.MemberActivityStatus
            })
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
            .Select(m => new MemberResponseDto
            {
                Id = m.Id,
                Name = m.Name,
                FirstName = m.FirstName,
                LastName = m.LastName,
                OtherNames =  m.OtherNames,
                EmailAddress = m.EmailAddress,
                PhoneNumber = m.PhoneNumber,
                DateOfBirth = m.DateOfBirth,
                JoinedDate =  m.JoinedDate,
                Gender = m.Gender,
                ResidentialAddress =  m.ResidentialAddress,
                City = m.City,
                Hometown =  m.Hometown,
                Region = m.Region,
                GpsAddress =  m.GpsAddress,
                MaritalStatus =   m.MaritalStatus,
                NextOfKin =  m.NextOfKin,
                EmergencyContactName = m.EmergencyContactName,
                EmergencyContactPhoneNumber =  m.EmergencyContactPhoneNumber,
                MemberActivityStatus =  m.MemberActivityStatus,
                CreatedAt =   m.CreatedAt
            })
            .FirstOrDefaultAsync(ct);
    }
}
