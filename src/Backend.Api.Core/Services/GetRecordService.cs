using Backend.Api.Core.Data;
using Backend.Api.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Services;

public class GetRecordService<T> : IGetRecordService<T> where T : class, ICongregationEntity, IDateTrackedEntity, ISearchableEntity
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GetRecordService(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetRecordByIdAsync(Guid Id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(m => m.Id == Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public int GetTotalRecordsCount(QueryParameters queryParameters)
    {
        return _dbSet
            .AsNoTracking()
            .ApplyFilters(queryParameters)
            .Count();
    }

    public PagedResponse<T> CreateNewPagedResponseObject(
        List<T> records, PaginationParams paginationParameters, int totalRecords)
    {
        return new PagedResponse<T>(
            records, paginationParameters.PageNumber, paginationParameters.PageSize, totalRecords);
    }
}
