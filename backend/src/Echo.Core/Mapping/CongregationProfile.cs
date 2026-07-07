using AutoMapper;
using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping;

public class CongregationProfile : Profile
{
    public CongregationProfile()
    {
        CreateMap<CongregationCreateDto, Congregation>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        CreateMap<CongregationCreateDto, Congregation>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());
    }
}
