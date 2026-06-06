using AutoMapper;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Backend.Api.Core.Services.Base;

namespace Backend.Api.Core.Services;

public class MemberService : ServiceBase<Member>
{
    public MemberService(RelationshipRepositoryBase<Member> repository, IMapper mapper) : base(repository, mapper)
    {
    }


}
