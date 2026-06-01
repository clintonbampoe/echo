using AutoMapper;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Interfaces;
using Backend.Api.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

public class OrganizationRepository : EntityRepositoryBase<Organization>
{
    public OrganizationRepository(DbContext context, IMapper mapper, IDomainRecordService<Organization> domainRecordService)
        : base(context, mapper, domainRecordService)
    {
    }
}

