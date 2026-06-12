using AutoMapper;
using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Common.Query;
using Backend.Api.Core.Data;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

public class AssetRepository(AppDbContext context, IMapper mapper)
    : RepositoryBase<Asset>(context, mapper)
{
    public override async Task<PagedResponse<Asset>> GetPageAsync(
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var totalRecords = await _dbSet.AsNoTracking().CountAsync(ct);

        var records = await _dbSet
            .AsNoTracking()
            .Include(a => a.Category)
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<Asset>(records, paginationParameters, totalRecords);
    }

    public override async Task<Asset?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Include(a => a.Category)
            .FirstOrDefaultAsync(ct);
    }
}
