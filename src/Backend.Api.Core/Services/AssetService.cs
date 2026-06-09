using AutoMapper;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories;
using Backend.Api.Core.Services.Base;

namespace Backend.Api.Core.Services;

public class AssetService(AssetRepository repository, IMapper mapper)
    : ServiceBase<Asset>(repository, mapper) { }
