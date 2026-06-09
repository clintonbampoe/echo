using System.Linq.Expressions;
using AutoMapper;
using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Data;
using Backend.Api.Core.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories.Base;

public abstract class RelationshipRepositoryBase<T>(AppDbContext context, IMapper mapper)
    : RepositoryBase<T>(context: context, mapper)
    where T : ICongregationEntity, ISoftDeletableEntity
{
    public virtual async Task<PagedResponse<T>> GetPageByForeignKeyPropertyId(
        Expression<Func<T, bool>> predicate,
        PaginationParameters paginationParameters,
        CancellationToken ct
    )
    {
        var query = _dbSet.Where(predicate);

        int totalRecordCount = await query.CountAsync(ct);

        var entities = await query.ApplyPagination(paginationParameters).ToListAsync(ct);

        return new PagedResponse<T>(entities, paginationParameters, totalRecordCount);
    }
}
