using AutoMapper;
using Backend.Api.Core.Entities.Dtos;

namespace Backend.Api.Core.Common.Mapping;

public class MemberProfile : Profile
{
    public MemberProfile()
    {
        CreateMap<Entities.Member, MemberResponseDto>().ReverseMap();
        CreateMap<Entities.Member, MemberCreateDto>().ReverseMap();
        CreateMap<Entities.Member, MemberUpdateDto>().ReverseMap();

        CreateMap<Entities.Member, MemberListResponseDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                $"{src.FirstName} {src.LastName} {src.OtherNames}"))
            .ReverseMap()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.FirstName, opt => opt.Ignore())
            .ForMember(dest => dest.LastName, opt => opt.Ignore())
            .ForMember(dest => dest.OtherNames, opt => opt.Ignore());
    }
}
