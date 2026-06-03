using AutoMapper;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Common.Mapping;

public class OrganizationProfile : Profile
{
    public OrganizationProfile()
    {
        CreateMap<Organization, OrganizationResponseDto>().ReverseMap();
        CreateMap<Organization, OrganizationListResponseDto>().ReverseMap();
        CreateMap<Organization, OrganizationCreateDto>()
            .ReverseMap()
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<OrganizationUpdateDto, Organization>().ReverseMap();
        CreateMap<OrganizationDeleteDto, Organization>().ReverseMap();
    }
}
