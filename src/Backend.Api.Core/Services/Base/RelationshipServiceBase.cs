using System.Linq.Expressions;
using AutoMapper;
using Backend.Api.Core.Common.HttpResults;
using Backend.Api.Core.Common.HttpResults.Interfaces;
using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Repositories.Base;

namespace Backend.Api.Core.Services.Base;

public abstract class RelationshipServiceBase<T> : ServiceBase<T>
    where T : class, ICongregationEntity, ISoftDeletableEntity
{
    protected override RelationshipRepositoryBase<T> Repository { get; }
    protected RelationshipServiceBase(RelationshipRepositoryBase<T> repository, IMapper mapper)
        : base(repository, mapper)
    {
        Repository = repository;
    }

    public virtual async Task<IOperationResult> GetPageByForeignKeyPropertyId(
            Expression<Func<T, bool>> predicate,
            PaginationParameters paginationParameters,
            CancellationToken ct
        )
    {
        var pagedEntities = await Repository.GetPageByForeignKeyPropertyId(
            predicate,
            paginationParameters,
            ct
        );

        var responseDtos = _mapper.Map<List<IListResponseDto<T>>>(pagedEntities.Data);

        var pagedDtos = new PagedResponse<IListResponseDto<T>>(
            responseDtos,
            paginationParameters,
            pagedEntities.TotalRecordCount
        );

        return new SuccessResult<PagedResponse<IListResponseDto<T>>>(pagedDtos);
    }
}
