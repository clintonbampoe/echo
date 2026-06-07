using AutoMapper;
using Backend.Api.Core.Data;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Backend.Api.Core.Repositories.Engines.Interfaces;

namespace Backend.Api.Core.Repositories;

public class AssetCategoryRepository : RepositoryBase<AssetCategory>
{
    public AssetCategoryRepository(AppDbContext context, IMapper mapper, IDatabaseEngine<AssetCategory> domainRecordService) : base(context, mapper, domainRecordService)
    {
    }
}
