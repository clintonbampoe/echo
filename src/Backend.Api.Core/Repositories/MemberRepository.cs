using AutoMapper;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Backend.Api.Core.Repositories.Engines.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Repositories;

public class MemberRepository : EntityRepositoryBase<Member>
{
    public MemberRepository(DbContext context, IMapper mapper, IDatabaseEngine<Member> domainRecordService
    ) : base(context, mapper, domainRecordService)
    {

    }
}
