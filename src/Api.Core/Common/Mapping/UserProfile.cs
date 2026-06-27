using Api.Core.Dtos;
using Api.Core.Entities;
using AutoMapper;

namespace Api.Core.Common.Mapping;

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
