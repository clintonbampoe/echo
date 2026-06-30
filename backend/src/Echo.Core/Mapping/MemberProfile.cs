using AutoMapper;
using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping;

public class MemberProfile : Profile
{
    public MemberProfile()
    {
        CreateMap<MemberCreateDto, Member>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(
                dest => dest.Name,
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim())
            )
            .ForMember(dest => dest.CongregationId, opt => opt.Ignore())
            .ForMember(dest => dest.Congregation, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        CreateMap<MemberUpdateDto, Member>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(
                dest => dest.Name,
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim())
            )
            .ForMember(dest => dest.CongregationId, opt => opt.Ignore())
            .ForMember(dest => dest.Congregation, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());
    }
}
