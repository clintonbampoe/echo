using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Common.Query;
using Backend.Api.Core.Data;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

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
            .Where(e => e.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .ApplyPagination(paginationParameters)
            .Select(e => new EventListResponseDto(
                e.Id,
                e.Organization.Name,
                e.Organizer.Name,
                e.Name,
                e.StartDate,
                e.EndDate,
                e.Location
            ))
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
