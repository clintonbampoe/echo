using AutoMapper;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Backend.Api.Core.Repository;
using Backend.Api.Core.Services.Base;

namespace Backend.Api.Core.Services;

public class ProjectCategoryService : ServiceBase<ProjectCategory>
{
    public ProjectCategoryService(ProjectCategoryRepository repository, IMapper mapper) : base(repository, mapper)
    {
    }
}
