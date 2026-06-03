using Backend.Api.Core.Entities;
using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Services.Interfaces;

public interface IDatabaseEngine<T> : IGetEntityEngine<T>, ICreateEntityEngine<T>, ISoftDeleteEntityEngine<T>, IUpdateEntityEngine<T>
    where T : class, ICongregationEntity, ISoftDeletableEntity
{

}
