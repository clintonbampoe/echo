using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Common.Query;
using Backend.Api.Core.Data;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

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
        var query = _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(e => e.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .ApplyPagination(paginationParameters)
            .Select(e => new EventRegistrationListResponseDto(
                e.Id,
                e.Member.Name,
                e.Event.Name,
                e.RegistrationDate
            ))
            .ToListAsync(ct);

        return new PagedResponse<EventRegistrationListResponseDto>(
            records,
            paginationParameters,
            totalRecords
        );
    }

    public async Task<EventRegistrationResponseDto?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default
    )
    {
        return await _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(e => e.Id == id)
            .Select(e => new EventRegistrationResponseDto(
                e.Id,
                e.MemberId,
                e.Member.Name,
                e.EventId,
                e.Event.Name,
                e.RegistrationDate,
                e.CreatedAt
            ))
            .FirstOrDefaultAsync(ct);
    }
}
