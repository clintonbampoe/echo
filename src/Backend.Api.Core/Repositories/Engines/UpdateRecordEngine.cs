using Backend.Api.Core.Data;
using Backend.Api.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Services;

public class UpdateRecordEngine<T> : IUpdateEntityEngine<T> where T : class, ICongregationEntity
{
    private readonly AppDbContext _appDbContext;
    private readonly DbSet<T> _dbSet;

    public UpdateRecordEngine(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
        _dbSet = appDbContext.Set<T>();
    }
    public async Task<bool> UpdateEntityById(Guid Id, T updatedRecordData, CancellationToken cancellationToken = default)
    {
        var existingRecord =
            await _dbSet.FirstOrDefaultAsync(rec => rec.Id == Id);

        if (existingRecord is null)
            return false;

        _dbSet.Entry(existingRecord).CurrentValues.SetValues(updatedRecordData);
        return true;
    }

}
