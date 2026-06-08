using AutoMapper;
using Backend.Api.Core.Data;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Backend.Api.Core.Repositories.Engines.Interfaces;

namespace Backend.Api.Core.Repositories;

public class ProjectContributionRepository : RelationshipRepositoryBase<ProjectContribution>
{
    public ProjectContributionRepository(
        AppDbContext context,
        IMapper mapper,
        IDatabaseEngine<ProjectContribution> domainRecordService
    )
        : base(context, mapper, domainRecordService) { }
}
