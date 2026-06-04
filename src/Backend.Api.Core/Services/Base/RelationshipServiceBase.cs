using AutoMapper;
using Backend.Api.Core.Common.HttpResults.Interfaces;
using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Repositories.Base;

namespace Backend.Api.Core.Services.Base;

public abstract class RelationshipServiceBase<T> : ServiceBase<T>
    where T : class, ICongregationEntity, ISoftDeletableEntity
{
    protected RelationshipServiceBase(EntityRepositoryBase<T> entityRepositoryBase, IMapper mapper) : base(entityRepositoryBase, mapper)
    {
    }
}
