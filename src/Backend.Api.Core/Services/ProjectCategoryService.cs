using AutoMapper;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repository;
using Backend.Api.Core.Services.Base;

namespace Backend.Api.Core.Services;

public class ProjectCategoryService(ProjectCategoryRepository repository, IMapper mapper)
    : ServiceBase<ProjectCategory>(repository, mapper) { }
