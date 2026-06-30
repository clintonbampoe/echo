using AutoMapper;
using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserCreateDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Congregation, opt => opt.Ignore())
            .ForMember(dest => dest.CongregationId, opt => opt.Ignore());

        CreateMap<UserUpdateDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Congregation, opt => opt.Ignore())
            .ForMember(dest => dest.CongregationId, opt => opt.Ignore());
    }
}
