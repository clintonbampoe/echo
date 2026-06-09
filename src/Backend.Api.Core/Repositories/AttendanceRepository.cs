using AutoMapper;
using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Common.Query;
using Backend.Api.Core.Data;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

public class AttendanceRepository(AppDbContext context, IMapper mapper)
    : RepositoryBase<AttendanceRecord>(context, mapper)
{
    public override async Task<PagedResponse<AttendanceRecord>> GetPageAsync(
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var totalRecords = await _dbSet.AsNoTracking().CountAsync(ct);

        var records = await _dbSet
            .AsNoTracking()
            .Include(a => a.Member)
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<AttendanceRecord>(records, paginationParameters, totalRecords);
    }

    public override async Task<AttendanceRecord?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Include(a => a.Member)
            .FirstOrDefaultAsync(ct);
    }
}
