using Echo.Application.Extensions.QueryMethods;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Core.Dtos;
using Echo.Core.Repositories.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Echo.Core.Repositories;

public class EventRepository(AppDbContext context) : PrimaryRepositoryBase<Event>(context)
{
    public async Task<PagedResponse<EventListResponseDto>> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var query = DbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .ApplySearchFilter(queryParameters)
            .ApplyDateFilters(queryParameters)
            .Where(e => e.CongregationId == congregationId);

        int totalRecords = await query.CountAsync(ct);

        var records = await query
            .OrderBy(e => e.Id)
            .Select(e => new EventListResponseDto
            {
                Id = e.Id,
                OrganizationName = e.Organization.Name,
                OrganizerName = e.Organizer.Name,
                Name = e.Name,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Location = e.Location,
            })
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<EventListResponseDto>(records, paginationParameters, totalRecords);
    }

    public async Task<EventResponseDto?> GetByIdAsync(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        return await DbSet
            .AsNoTracking()
            .ApplySoftDeleteFilter()
            .Where(e => e.Id == id && e.CongregationId == congregationId)
            .Select(e => new EventResponseDto
            {
                Id = e.Id,
                OrganizationId = e.OrganizationId,
                OrganizationName = e.Organization.Name,
                OrganizerId = e.Organizer.Id,
                OrganizerName = e.Organizer.Name,
                Name = e.Name,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Location = e.Location,
                Capacity = e.Capacity,
                Description = e.Description,
                CreatedAt = e.CreatedAt,
            })
            .FirstOrDefaultAsync(ct);
    }
}
