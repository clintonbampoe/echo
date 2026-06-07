using AutoMapper;
using Backend.Api.Core.Data;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Backend.Api.Core.Repositories.Engines.Interfaces;

namespace Backend.Api.Core.Repositories;

public class AssetRepository : RepositoryBase<Asset>
{
    public AssetRepository(AppDbContext context, IMapper mapper, IDatabaseEngine<Asset> domainRecordService) : base(context, mapper, domainRecordService)
    {
    }
}
