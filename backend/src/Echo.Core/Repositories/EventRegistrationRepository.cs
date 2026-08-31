using Echo.Application.Extensions.QueryMethods;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class EventRegistrationRepository(AppDbContext context)
    : PrimaryRepositoryBase<EventRegistration>(context)
{
    public async Task<PagedResponse<EventRegistrationListResponseDto>> GetPageAsync(
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
            .Select(e => new EventRegistrationListResponseDto
            {
                Id = e.Id,
                MemberName = e.Member.Name,
                EventName = e.Event.Name,
                RegistrationDate = e.RegistrationDate,
            })
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<EventRegistrationListResponseDto>(
            records,
            paginationParameters,
            totalRecords
        );
    }

    public async Task<EventRegistrationResponseDto?> GetByIdAsync(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await DbSet
            .AsNoTracking()
            .Where(e => e.Id == id && e.CongregationId == congregationId)
            .Select(e => new EventRegistrationResponseDto
            {
                Id = e.Id,
                MemberId = e.MemberId,
                MemberName = e.Member.Name,
                EventId = e.EventId,
                EventName = e.Event.Name,
                RegistrationDate = e.RegistrationDate,
                CreatedAt = e.CreatedAt,
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResponse<EventRegistrationListResponseDto>> GetByMemberId(
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
            .Select(e => new EventRegistrationListResponseDto
            {
                Id = e.Id,
                MemberName = e.Member.Name,
                EventName = e.Event.Name,
                RegistrationDate = e.RegistrationDate,
            })
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<EventRegistrationListResponseDto>(
            records,
            paginationParameters,
            totalCount
        );
    }

    public async Task<PagedResponse<EventRegistrationListResponseDto>> GetByEventId(
        PaginationParameters paginationParameters,
        QueryParameters queryParameters,
        Guid eventId,
        CancellationToken ct
    )
    {
        var query = DbSet
            .AsNoTracking()
            .ApplyDateFilters(queryParameters)
            .Where(e => e.MemberId == eventId);

        var totalCount = await query.CountAsync(ct);

        var records = await query
            .OrderBy(e => e.Id)
            .Select(e => new EventRegistrationListResponseDto
            {
                Id = e.Id,
                MemberName = e.Member.Name,
                EventName = e.Event.Name,
                RegistrationDate = e.RegistrationDate,
            })
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<EventRegistrationListResponseDto>(
            records,
            paginationParameters,
            totalCount
        );
    }
}
