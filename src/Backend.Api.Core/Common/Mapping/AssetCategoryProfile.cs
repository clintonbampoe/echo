using AutoMapper;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Common.Mapping;

public class AssetCategoryProfile : Profile
{
    public AssetCategoryProfile()
    {
        // Outbound only
        CreateMap<AssetCategory, AssetCategoryResponseDto>();
        CreateMap<AssetCategory, AssetCategoryListResponseDto>();

        // Inbound
        CreateMap<AssetCategoryCreateDto, AssetCategory>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        CreateMap<AssetCategoryUpdateDto, AssetCategory>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        CreateMap<AssetCategoryDeleteDto, AssetCategory>().ForAllMembers(opt => opt.Ignore());
    }
}
