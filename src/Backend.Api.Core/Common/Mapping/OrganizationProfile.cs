using AutoMapper;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Entities.Dtos;

namespace Backend.Api.Core.Common.Mapping;

public class OrganizationProfile : Profile
{
    public OrganizationProfile()
    {
        CreateMap<Organization, OrganizationResponseDto>().ReverseMap();
        CreateMap<Organization, OrganizationListResponseDto>().ReverseMap();
        CreateMap<OrganizationCreateDto, Organization>().ReverseMap();
        CreateMap<OrganizationUpdateDto, Organization>().ReverseMap();
        CreateMap<OrganizationDeleteDto, Organization>().ReverseMap();
    }
}
