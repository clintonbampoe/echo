using AutoMapper;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Common.Mapping;

public class TitheProfile : Profile
{
    public TitheProfile()
    {
        CreateMap<Tithe, TitheResponseDto>()
            .ForMember(dest => dest.Member, opt => opt.MapFrom(src => src.Member.Name));

        CreateMap<Tithe, TitheListResponseDto>()
            .ForMember(dest => dest.Member, opt => opt.MapFrom(src => src.Member.Name));

        CreateMap<TitheCreateDto, Tithe>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Member, opt => opt.Ignore());

        CreateMap<TitheUpdateDto, Tithe>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Member, opt => opt.Ignore());

        CreateMap<TitheDeleteDto, Tithe>().ForAllMembers(opt => opt.Ignore());
    }
}
