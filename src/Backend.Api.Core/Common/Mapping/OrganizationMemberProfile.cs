using AutoMapper;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Common.Mapping;

public class OrganizationMemberProfile : Profile
{
    public OrganizationMemberProfile()
    {
        // Outbound only
        CreateMap<OrganizationMember, OrganizationMemberResponseDto>()
            .ForMember(dest => dest.Member, opt => opt.MapFrom(src => src.Member.Name))
            .ForMember(dest => dest.Organization, opt => opt.MapFrom(src => src.Organization.Name));

        CreateMap<OrganizationMember, OrganizationMemberListResponseDto>()
            .ForMember(dest => dest.Member, opt => opt.MapFrom(src => src.Member.Name))
            .ForMember(dest => dest.Organization, opt => opt.MapFrom(src => src.Organization.Name));

        // Inbound
        CreateMap<OrganizationMemberCreateDto, OrganizationMember>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Member, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore());

        CreateMap<OrganizationMemberUpdateDto, OrganizationMember>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Member, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore());

        CreateMap<OrganizationMemberDeleteDto, OrganizationMember>()
            .ForAllMembers(opt => opt.Ignore());
    }
}
