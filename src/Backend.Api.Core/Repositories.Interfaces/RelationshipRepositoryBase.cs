using AutoMapper;
using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories.Interfaces;

public abstract class RelationshipRepositoryBase<T> : EntityRepositoryBase<T>
    where T : class, ICongregationEntity, ISoftDeletableEntity
{
    protected RelationshipRepositoryBase(DbContext context, IMapper mapper, IDomainRecordService<T> domainRecordService)
        : base(context, mapper, domainRecordService)
    {
    }

    public virtual async Task<PagedResponse<T>> GetPageByLeftEntityIdAsync(
        Guid leftId,
        PaginationParams paginationParameters,
        QueryParameters queryParameters
        )
    {
        var totalRecordCount = await _dbSet
            .AsNoTracking()
            .CountAsync();

        var records = await _dbSet
            .AsNoTracking()
            .Where(r => r.CongregationId == leftId)
            .ApplyPagination(paginationParameters)
            .ToListAsync();

        return _domainRecordService.CreateNewPagedResponseObject(records, paginationParameters, totalRecordCount);
    }

    public virtual async Task<PagedResponse<T>> GetPageByRightEntityIdAsync(
        Guid rightId,
        PaginationParams paginationParameters,
        QueryParameters queryParameters
        )
    {
        var totalRecordCount = await _dbSet
            .AsNoTracking()
            .CountAsync();

        var records = await _dbSet
            .AsNoTracking()
            .Where(r => r.CongregationId == rightId)
            .ApplyPagination(paginationParameters)
            .ToListAsync();

        return _domainRecordService.CreateNewPagedResponseObject(records, paginationParameters, totalRecordCount);
    }
}
