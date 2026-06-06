using AutoMapper;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories;
using Backend.Api.Core.Repositories.Base;
using Backend.Api.Core.Services.Base;

namespace Backend.Api.Core.Services;

public class ProjectService : ServiceBase<Project>
{
    public ProjectService(ProjectRepository repository, IMapper mapper) : base(repository, mapper)
    {
    }
}
