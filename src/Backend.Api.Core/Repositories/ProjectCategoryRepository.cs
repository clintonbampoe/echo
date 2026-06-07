using AutoMapper;
using Backend.Api.Core.Data;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Backend.Api.Core.Repositories.Engines.Interfaces;

namespace Backend.Api.Core.Repository;

public class ProjectCategoryRepository : RepositoryBase<ProjectCategory>
{
    public ProjectCategoryRepository(AppDbContext context, IMapper mapper, IDatabaseEngine<ProjectCategory> domainRecordService) : base(context, mapper, domainRecordService)
    {
    }
}
