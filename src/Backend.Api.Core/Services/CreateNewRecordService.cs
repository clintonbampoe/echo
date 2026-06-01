using Backend.Api.Core.Data;
using Backend.Api.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Services;

public class CreateNewRecordService<T> : ICreateNewRecordService<T> where T : class, ICongregationEntity
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public CreateNewRecordService(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<bool> CreateNewRecord(T newRecordData, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(newRecordData, cancellationToken);
        return true;
    }
}
