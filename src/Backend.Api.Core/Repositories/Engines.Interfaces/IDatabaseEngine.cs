using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Repositories.Engines.Interfaces;

public interface IDatabaseEngine<T>
    : IGetEntityEngine<T>,
        ICreateEntityEngine<T>,
        ISoftDeleteEntityEngine<T>,
        IUpdateEntityEngine<T>
    where T : ICongregationEntity, ISoftDeletableEntity { }
