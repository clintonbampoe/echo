using AutoMapper;
using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Common.Query;
using Backend.Api.Core.Data;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

public class ProjectRepository(AppDbContext context, IMapper mapper)
    : RepositoryBase<Project>(context, mapper)
{
    public override async Task<PagedResponse<Project>> GetPageAsync(
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken cancellationToken = default
    )
    {
        var totalRecordCount = await _dbSet
            .AsNoTracking()
            .ApplySearchFilter(queryParameters)
            .ApplyDateFilters(queryParameters)
            .CountAsync(cancellationToken);

        var records = await _dbSet
            .AsNoTracking()
            .ApplySearchFilter(queryParameters)
            .ApplyDateFilters(queryParameters)
            .Include(p => p.Category)
            .Include(p => p.Manager)
            .ApplyPagination(paginationParameters)
            .ToListAsync(cancellationToken);

        return new PagedResponse<Project>(records, paginationParameters, totalRecordCount);
    }

    public override async Task<Project?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Include(p => p.Category)
            .Include(p => p.Manager)
            .FirstOrDefaultAsync(ct);
    }
}
