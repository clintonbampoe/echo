using Backend.Api.Core.Data;
using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Repositories.Engines.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories.Engines;

public class SoftDeleteRecordEngine<T> : ISoftDeleteEntityEngine<T> where T : class, ICongregationEntity, ISoftDeletableEntity
{
    private readonly AppDbContext _appDbContext;
    private readonly DbSet<T> _dbSet;

    public SoftDeleteRecordEngine(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
        _dbSet = appDbContext.Set<T>();
    }

    public async Task<bool> SoftDeleteByIdAsync(Guid Id, CancellationToken cancellationToken = default)
    {
        var existingRecord = await _dbSet.FirstOrDefaultAsync(rec => rec.Id == Id);

        if (existingRecord is null)
            return false;

        existingRecord.DeletedAt = DateTime.UtcNow;
        return true;
    }
}
