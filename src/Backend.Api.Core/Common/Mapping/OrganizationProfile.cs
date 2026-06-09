using AutoMapper;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Common.Mapping;

public class OrganizationProfile : Profile
{
    public OrganizationProfile()
    {
        // Outbound only
        CreateMap<Organization, OrganizationResponseDto>();
        CreateMap<Organization, OrganizationListResponseDto>();

        // Inbound
        CreateMap<OrganizationCreateDto, Organization>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        CreateMap<OrganizationUpdateDto, Organization>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        CreateMap<OrganizationDeleteDto, Organization>().ForAllMembers(opt => opt.Ignore());
    }
}
