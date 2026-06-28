using Api.Core.Common.Pagination;
using Api.Core.Common.Query;
using Api.Core.Data;
using Api.Core.Dtos;
using Api.Core.Entities;
using Api.Core.Repositories.Base;
using Api.Core.Common.ExtensionMethods;
using Api.Core.Dtos.Core;
using Api.Core.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Repositories;

public class EventRepository(AppDbContext context) : PrimaryRepositoryBase<Event>(context)
{
    public async Task<PagedResponse<EventListResponseDto>> GetPageAsync(
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
            .Where(e => e.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .OrderBy(e => e.Id)
            .Select(e => new EventListResponseDto(
                e.Id,
                e.Organization.Name,
                e.Organizer.Name,
                e.Name,
                e.StartDate,
                e.EndDate,
                e.Location
            ))
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<EventListResponseDto>(records, paginationParameters, totalRecords);
    }

    public async Task<EventResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(e => e.Id == id)
            .Select(e => new EventResponseDto(
                e.Id,
                e.OrganizationId,
                e.Organization.Name,
                e.OrganizerId,
                e.Organizer.Name,
                e.Name,
                e.StartDate,
                e.EndDate,
                e.StartTime,
                e.EndTime,
                e.Location,
                e.Capacity,
                e.Description,
                e.CreatedAt
            ))
            .FirstOrDefaultAsync(ct);
    }
}
