using AutoMapper;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Backend.Api.Core.Repositories.Engines.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

public class OrganizationRepository : EntityRepositoryBase<Organization>
{
    public OrganizationRepository(DbContext context, IMapper mapper, IDatabaseEngine<Organization> domainRecordService)
        : base(context, mapper, domainRecordService)
    {
    }
}

