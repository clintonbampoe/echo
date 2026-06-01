using Backend.Api.Core.Data;
using Backend.Api.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Services;

public class CreateNewRecordService<T> : ICreateNewRecordService<T> where T : class, ICongregationEntity
{
    private readonly AppDbContext _appDbContext;
    private readonly DbSet<T> _dbSet;

    public CreateNewRecordService(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
        _dbSet = appDbContext.Set<T>();
    }

    public async Task<bool> CreateNewRecord(T newRecordData, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(newRecordData, cancellationToken);
        return true;
    }
}
