using Backend.Api.Core.Data;
using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Repositories.Engines.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories.Engines;

public class CreateNewRecordEngine<T> : ICreateEntityEngine<T> where T : class, ICongregationEntity
{
    private readonly AppDbContext _appDbContext;
    private readonly DbSet<T> _dbSet;

    public CreateNewRecordEngine(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
        _dbSet = appDbContext.Set<T>();
    }

    public async Task<bool> CreateNewEntity(T newRecordData, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(newRecordData, cancellationToken);
        return true;
    }
}
