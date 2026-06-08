using AutoMapper;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Common.Mapping;

public class AssetProfile : Profile
{
    public AssetProfile()
    {
        // Outbound only
        CreateMap<Asset, AssetResponseDto>()
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.Name));

        CreateMap<Asset, AssetListResponseDto>()
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.Name));

        // Inbound
        CreateMap<AssetCreateDto, Asset>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore());

        CreateMap<AssetUpdateDto, Asset>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore());

        CreateMap<AssetDeleteDto, Asset>().ForAllMembers(opt => opt.Ignore());
    }
}
