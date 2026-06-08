using AutoMapper;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Common.Mapping;

public class EventRegistrationProfile : Profile
{
    public EventRegistrationProfile()
    {
        // Outbound only
        CreateMap<EventRegistration, EventRegistrationResponseDto>()
            .ForMember(dest => dest.Member, opt => opt.MapFrom(src => src.Member.Name))
            .ForMember(dest => dest.Event, opt => opt.MapFrom(src => src.Event.Name));

        CreateMap<EventRegistration, EventRegistrationListResponseDto>()
            .ForMember(dest => dest.Member, opt => opt.MapFrom(src => src.Member.Name))
            .ForMember(dest => dest.Event, opt => opt.MapFrom(src => src.Event.Name));

        // Inbound
        CreateMap<EventRegistrationCreateDto, EventRegistration>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Member, opt => opt.Ignore())
            .ForMember(dest => dest.Event, opt => opt.Ignore());

        CreateMap<EventRegistrationUpdateDto, EventRegistration>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Member, opt => opt.Ignore())
            .ForMember(dest => dest.Event, opt => opt.Ignore());

        CreateMap<EventRegistrationDeleteDto, EventRegistration>()
            .ForAllMembers(opt => opt.Ignore());
    }
}
