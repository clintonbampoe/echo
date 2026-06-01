using Backend.Api.Core.Data;
using Backend.Api.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Services;

public class UpdateRecordService<T> : IUpdateRecordService<T> where T : class, ICongregationEntity
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public UpdateRecordService(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<bool> UpdateRecord(
        Guid Id, T updatedRecordData, CancellationToken cancellationToken = default)
    {
        var existingRecord = await _dbSet.FirstOrDefaultAsync(m => m.Id == Id, cancellationToken);

        if (existingRecord is null)
            return false;

        existingRecord = updatedRecordData;

        return true;
    }
}
