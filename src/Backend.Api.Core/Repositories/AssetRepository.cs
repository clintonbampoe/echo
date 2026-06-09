using AutoMapper;
using Backend.Api.Core.Data;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;

namespace Backend.Api.Core.Repositories;

public class AssetRepository(AppDbContext context, IMapper mapper)
    : RepositoryBase<Asset>(context, mapper) { }
