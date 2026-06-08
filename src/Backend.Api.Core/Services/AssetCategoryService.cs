using AutoMapper;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories;
using Backend.Api.Core.Services.Base;

namespace Backend.Api.Core.Services;

public class AssetCategoryService(AssetCategoryRepository repository, IMapper mapper)
    : ServiceBase<AssetCategory>(repository, mapper) { }
