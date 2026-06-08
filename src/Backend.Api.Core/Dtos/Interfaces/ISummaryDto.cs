using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Dtos.Interfaces;

public interface ISummaryDto<T>
    where T : ICongregationEntity, ISoftDeletableEntity { }
