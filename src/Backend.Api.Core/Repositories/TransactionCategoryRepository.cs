using AutoMapper;
using Backend.Api.Core.Data;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Backend.Api.Core.Repositories.Engines.Interfaces;

namespace Backend.Api.Core.Repositories;

public class TransactionCategoryRepository : RepositoryBase<TransactionCategory>
{
    public TransactionCategoryRepository(AppDbContext context, IMapper mapper, IDatabaseEngine<TransactionCategory> domainRecordService) : base(context, mapper, domainRecordService)
    {
    }
}
