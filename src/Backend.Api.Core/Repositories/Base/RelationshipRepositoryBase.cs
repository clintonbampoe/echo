using AutoMapper;
using Backend.Api.Core.Common.ExtensionMethods;
using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Repositories.Engines.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories.Base;

public abstract class RelationshipRepositoryBase<T> : EntityRepositoryBase<T>
    where T : class, ICongregationEntity, ISoftDeletableEntity
{
    protected RelationshipRepositoryBase(DbContext context, IMapper mapper, IDatabaseEngine<T> databaseEngine)
        : base(context, mapper, databaseEngine)
    {
    }

    protected PagedResponse<T> GetPageByForeignKeyPropertyId<TForeignKeyProperty>(
            Guid foreignKeyId,
            PaginationParameters paginationParameters,
            CancellationToken ct
        )
    {
        throw new NotImplementedException();
    }

}
