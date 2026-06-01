using Backend.Api.Core.Entities;
using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Services.Interfaces;

public interface IDomainRecordService<T> : IGetRecordService<T>, ICreateNewRecordService<T>, ISoftDeleteService<T>, IUpdateRecordService<T>
    where T : class, ICongregationEntity, ISoftDeletableEntity
{

}
