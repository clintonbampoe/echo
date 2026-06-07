using AutoMapper;
using Backend.Api.Core.Data;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories;
using Backend.Api.Core.Repositories.Base;
using Backend.Api.Core.Repositories.Engines.Interfaces;
using Backend.Api.Core.Services.Base;

namespace Backend.Api.Core.Services;

public class ProjectContributionService : RelationshipServiceBase<ProjectContribution>
{
    public ProjectContributionService(ProjectContributionRepository repository, IMapper mapper) : base(repository, mapper)
    {
    }
}
