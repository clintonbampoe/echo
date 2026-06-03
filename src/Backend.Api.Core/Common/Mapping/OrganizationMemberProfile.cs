using AutoMapper;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Common.Mapping;

public class OrganizationMemberProfile : Profile
{
    public OrganizationMemberProfile()
    {
        CreateMap<OrganizationMember, OrganizationMemberResponseDto>().ReverseMap();
        CreateMap<OrganizationMember, OrganizationMemberListResponseDto>()
            .ForMember(dest => dest.MemberName, opt => opt.MapFrom(src => src.Member.Name))
            .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization.Name))
            .ReverseMap()
            .ForMember(dest => dest.Member, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore());

        CreateMap<OrganizationMember, OrganizationMemberCreateDto>()
            .ReverseMap()
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<OrganizationMember, OrganizationMemberUpdateDto>().ReverseMap();
        CreateMap<OrganizationMember, OrganizationMemberDeleteDto>().ReverseMap();
    }
}
