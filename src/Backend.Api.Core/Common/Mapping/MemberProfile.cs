using AutoMapper;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Entities.Dtos;

namespace Backend.Api.Core.Common.Mapping;

public class MemberProfile : Profile
{
    public MemberProfile()
    {

        CreateMap<Member, MemberResponseDto>().ReverseMap();
        
        CreateMap<Member, MemberListResponseDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                $"{src.FirstName} {src.LastName} {src.OtherNames}"))
            .ReverseMap()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.FirstName, opt => opt.Ignore())
            .ForMember(dest => dest.LastName, opt => opt.Ignore())
            .ForMember(dest => dest.OtherNames, opt => opt.Ignore());

        CreateMap<Member, MemberCreateDto>()
            .ReverseMap()
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<Member, MemberUpdateDto>().ReverseMap();
        CreateMap<Member, MemberDeleteDto>().ReverseMap();
    }
}
