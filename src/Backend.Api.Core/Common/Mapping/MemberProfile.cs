using AutoMapper;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Common.Mapping;

public class MemberProfile : Profile
{
    public MemberProfile()
    {

        CreateMap<Member, MemberResponseDto>().ReverseMap();

        CreateMap<Member, MemberListResponseDto>();

        CreateMap<Member, MemberCreateDto>()
            .ReverseMap()
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<Member, MemberUpdateDto>().ReverseMap();
        CreateMap<Member, MemberDeleteDto>().ReverseMap();

        CreateMap<Member, IListResponseDto<Member>>()
                    .ConvertUsing((src, dest, context) => context.Mapper.Map<MemberListResponseDto>(src));
    }
}
