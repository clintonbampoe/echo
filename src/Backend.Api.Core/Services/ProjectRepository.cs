using AutoMapper;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Backend.Api.Core.Services.Base;

namespace Backend.Api.Core.Services;

public class ProjectService : ServiceBase<Project>
{
    public ProjectService(RepositoryBase<Project> repository, IMapper mapper) : base(repository, mapper)
    {
    }
}
