using AutoMapper;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Backend.Api.Core.Services.Base;

namespace Backend.Api.Core.Services;

public class OrganizationService : ServiceBase<Organization>
{
    public OrganizationService(EntityRepositoryBase<Organization> repository, IMapper mapper) : base(repository, mapper)
    {
    }
}
