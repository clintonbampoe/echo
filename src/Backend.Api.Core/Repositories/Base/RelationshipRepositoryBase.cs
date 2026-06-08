using System.Linq.Expressions;
using AutoMapper;
using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Data;
using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Repositories.Engines.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories.Base;

public abstract class RelationshipRepositoryBase<T> : RepositoryBase<T>
    where T : class, ICongregationEntity, ISoftDeletableEntity
{
    protected RelationshipRepositoryBase(
        AppDbContext context,
        IMapper mapper,
        IDatabaseEngine<T> databaseEngine
    )
        : base(context, mapper, databaseEngine) { }

    public virtual async Task<PagedResponse<T>> GetPageByForeignKeyPropertyId(
        Expression<Func<T, bool>> predicate,
        PaginationParameters paginationParameters,
        CancellationToken ct
    )
    {
        var query = _dbSet.Where(predicate);

        int totalRecordCount = await query.CountAsync(ct);

        var entities = await query.ApplyPagination(paginationParameters).ToListAsync();

        return _databaseEngine.CreateNewPagedResponseObject(
            entities,
            paginationParameters,
            totalRecordCount
        );
    }
}
