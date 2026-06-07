using AutoMapper;
using Backend.Api.Core.Data;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Backend.Api.Core.Repositories.Engines.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

public class OrganizationRepository : RepositoryBase<Organization>
{
    public OrganizationRepository(AppDbContext context, IMapper mapper, IDatabaseEngine<Organization> domainRecordService)
        : base(context, mapper, domainRecordService)
    {
    }
}

