using AutoMapper;
using AutoMapper.QueryableExtensions;
using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories.Interfaces;

public abstract class EntityRepositoryBase<T> where T : class, ICongregationEntity, ISoftDeletableEntity
{
    protected readonly DbContext _context;
    protected readonly DbSet<T> _dbSet;
    protected readonly IMapper _mapper;
    protected readonly IDatabaseEngine<T> _domainRecordService;

    public EntityRepositoryBase(
        DbContext context,
        IMapper mapper,
        IDatabaseEngine<T> domainRecordService
        )
    {
        _context = context;
        _dbSet = context.Set<T>();
        _mapper = mapper;
        _domainRecordService = domainRecordService;
    }

    public virtual async Task<PagedResponse<T>> GetPageAsync(
        PaginationParams paginationParameters,
        QueryParameters queryParameters,
        CancellationToken cancellationToken = default
        )
    {
        var totalRecords = await _dbSet
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var records = await _dbSet
            .AsNoTracking()
            .ApplyPagination(paginationParameters)
            .ToListAsync(cancellationToken);

        return _domainRecordService.CreateNewPagedResponseObject(records, paginationParameters, totalRecords);
    }

    public virtual async Task<T?> GetByIdAsync(Guid Id, CancellationToken cancellationToken = default)
    {
        return await _domainRecordService.GetEntityByIdAsync(Id, cancellationToken);
    }

    public virtual async Task<bool> CreateRecord(T newRecordData, CancellationToken cancellationToken = default)
    {
        var recordCreatedSuccessfully =
            await _domainRecordService.CreateNewEntity(newRecordData, cancellationToken);

        if (!recordCreatedSuccessfully)
            return false;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public virtual async Task<bool> UpdateRecord(Guid Id, T updatedRecordData, CancellationToken cancellationToken = default)
    {
        var recordUpdatedSuccessfully =
            await _domainRecordService.UpdateEntityById(Id, updatedRecordData, cancellationToken);

        if (!recordUpdatedSuccessfully)
            return false;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public virtual async Task<bool> DeleteRecord(Guid Id, CancellationToken cancellationToken = default)
    {
        var recordDeletedSuccessfully =
            await _domainRecordService.SoftDeleteByIdAsync(Id, cancellationToken);

        if (!recordDeletedSuccessfully)
            return false;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
