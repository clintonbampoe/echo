using AutoMapper;
using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Common.Query;
using Backend.Api.Core.Data;
using Backend.Api.Core.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories.Base;

public abstract class RepositoryBase<T>(AppDbContext context, IMapper mapper)
    where T : ICongregationEntity, ISoftDeletableEntity
{
    protected readonly AppDbContext _context = context;
    protected readonly DbSet<T> _dbSet = context.Set<T>();
    protected readonly IMapper _mapper = mapper;

    public virtual async Task<PagedResponse<T>> GetPageAsync(
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var totalRecords = await _dbSet.AsNoTracking().CountAsync(ct);

        var records = await _dbSet
            .AsNoTracking()
            .ApplyPagination(paginationParameters)
            .ToListAsync(ct);

        return new PagedResponse<T>(records, paginationParameters, totalRecords);
    }

    public virtual async Task<T?> GetByIdAsync(Guid Id, CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking().Where(e => e.Id == Id).FirstOrDefaultAsync(ct);
    }

    public virtual async Task<bool> CreateRecord(T newRecordData, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(newRecordData, ct);
        return true;
    }

    public virtual async Task<bool> UpdateRecord(
        Guid Id,
        T updatedRecordData,
        CancellationToken ct = default
    )
    {
        var existingRecord = await _dbSet.FirstOrDefaultAsync(rec => rec.Id == Id, ct);

        if (existingRecord is null)
            return false;

        _dbSet.Entry(existingRecord).CurrentValues.SetValues(updatedRecordData);
        return true;
    }

    public virtual async Task<bool> DeleteRecord(Guid Id, CancellationToken ct = default)
    {
        var existingRecord = await _dbSet.FirstOrDefaultAsync(rec => rec.Id == Id, ct);

        if (existingRecord is null)
            return false;

        existingRecord.DeletedAt = DateTime.UtcNow;
        return true;
    }
}
