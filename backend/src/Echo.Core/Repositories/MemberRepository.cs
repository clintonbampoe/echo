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

public class MemberRepository(AppDbContext context) : PrimaryRepositoryBase<Member>(context)
{
    public async Task<PagedResponse<MemberListResponseDto>> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var query = DbSet
            .AsNoTracking()
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
                PhoneNumber = m.PhoneNumber,
                EmailAddress = m.EmailAddress,
                Gender = m.Gender,
                MemberActivityStatus = m.MemberActivityStatus,
            })
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<MemberListResponseDto>(
            records,
            paginationParameters,
            totalRecords
        );
    }

    public async Task<MemberResponseDto?> GetByIdAsync(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await DbSet
            .AsNoTracking()
            .Where(m => m.Id == id && m.CongregationId == congregationId)
            .Select(m => new MemberResponseDto
            {
                Id = m.Id,
                Name = m.Name,
                FirstName = m.FirstName,
                LastName = m.LastName,
                OtherNames = m.OtherNames,
                EmailAddress = m.EmailAddress,
                PhoneNumber = m.PhoneNumber,
                DateOfBirth = m.DateOfBirth,
                JoinedDate = m.JoinedDate,
                Gender = m.Gender,
                ResidentialAddress = m.ResidentialAddress,
                City = m.City,
                Hometown = m.Hometown,
                Region = m.Region,
                GpsAddress = m.GpsAddress,
                MaritalStatus = m.MaritalStatus,
                NextOfKin = m.NextOfKin,
                EmergencyContactName = m.EmergencyContactName,
                EmergencyContactPhoneNumber = m.EmergencyContactPhoneNumber,
                MemberActivityStatus = m.MemberActivityStatus,
                CreatedAt = m.CreatedAt,
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<MemberSummaryDto> GetSummaryAsync(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateOnly(now.Year, now.Month, 1);

        var stats = await DbSet
            .Where(m => m.CongregationId == congregationId)
            .GroupBy(m => 1)
            .Select(g => new
            {
                TotalMembership = g.Count(m =>
                    m.MemberActivityStatus != MemberActivityStatus.Archived
                ),
                NewMembers = g.Count(m =>
                    m.JoinedDate != null && m.JoinedDate >= currentMonthStart
                ),
                Active = g.Count(m => m.MemberActivityStatus == MemberActivityStatus.Active),
                Countable = g.Count(m => m.MemberActivityStatus != MemberActivityStatus.Archived),
            })
            .FirstOrDefaultAsync(ct);

        var active = stats?.Active ?? 0;
        var countable = stats?.Countable ?? 0;

        return new MemberSummaryDto
        {
            TotalMembership = stats?.TotalMembership ?? 0,
            NewMembers = stats?.NewMembers ?? 0,
            RetentionRate = countable == 0 ? 0 : Math.Round((decimal)active / countable * 100, 1),
        };
    }

    public async Task<List<MemberListResponseDto>> SearchMembersByName(
        Guid congregationId,
        string searchString,
        CancellationToken ct
    )
    {
        var results = await DbSet
            .Where(m =>
                m.CongregationId == congregationId
                && EF.Functions.ILike(m.Name, $"%{searchString}%")
            )
            .OrderByDescending(m => EF.Functions.TrigramsSimilarity(m.Name, searchString))
            .Take(5)
            .Select(m => new MemberListResponseDto
            {
                Id = m.Id,
                Name = m.Name,
                PhoneNumber = m.PhoneNumber,
                EmailAddress = m.EmailAddress,
                Gender = m.Gender,
                MemberActivityStatus = m.MemberActivityStatus,
            })
            .ToListAsync(ct);

        return results;
    }
}
