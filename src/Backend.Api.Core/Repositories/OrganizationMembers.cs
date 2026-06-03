using AutoMapper;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Backend.Api.Core.Repositories.Engines.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

public class OrganizationMemberRepository : RelationshipRepositoryBase<OrganizationMember>
{
    public OrganizationMemberRepository(DbContext context, IMapper mapper, IDatabaseEngine<OrganizationMember> domainRecordService)
        : base(context, mapper, domainRecordService)
    {
    }
}
