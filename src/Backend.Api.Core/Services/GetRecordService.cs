using Backend.Api.Core.Data;
using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Services;

public class GetRecordService<T> : IGetRecordService<T> where T : class, ICongregationEntity
{
    private readonly AppDbContext _appDbContext;
    private readonly DbSet<T> _dbSet;

    public GetRecordService(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
        _dbSet = appDbContext.Set<T>();
    }
    public PagedResponse<T> CreateNewPagedResponseObject(List<T> records, PaginationParams paginationParameters, int totalRecords)
    {
        return new PagedResponse<T>(
            records, paginationParameters, totalRecords
        );
    }

    public async Task<T?> GetRecordByIdAsync(Guid Id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(e => e.Id == Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
