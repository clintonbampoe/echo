using AutoMapper;
using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Common.Query;
using Backend.Api.Core.Data;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

public class EventRepository(AppDbContext context, IMapper mapper)
    : RepositoryBase<Event>(context, mapper)
{
    public override async Task<PagedResponse<Event>> GetPageAsync(
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var totalRecords = await _dbSet.AsNoTracking().CountAsync(ct);

        var records = await _dbSet
            .AsNoTracking()
            .Include(a => a.Organizer)
            .Include(a => a.Organization)
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<Event>(records, paginationParameters, totalRecords);
    }

    public override async Task<Event?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Include(a => a.Organizer)
            .Include(a => a.Organization)
            .FirstOrDefaultAsync(ct);
    }
}
