using AutoMapper;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Backend.Api.Core.Services.Base;

namespace Backend.Api.Core.Services;

public class OrganizationMemberService : RelationshipServiceBase<OrganizationMember>
{
    public OrganizationMemberService(RelationshipRepositoryBase<OrganizationMember> repository, IMapper mapper)
        : base(repository, mapper)
    {
    }
}
