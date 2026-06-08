using AutoMapper;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Common.Mapping;

public class EventProfile : Profile
{
    public EventProfile()
    {
        // Outbound only
        CreateMap<Event, EventResponseDto>()
            .ForMember(dest => dest.Organization, opt => opt.MapFrom(src => src.Organization.Name))
            .ForMember(dest => dest.Organizer, opt => opt.MapFrom(src => src.Organizer.Name));

        CreateMap<Event, EventListResponseDto>()
            .ForMember(dest => dest.Organization, opt => opt.MapFrom(src => src.Organization.Name))
            .ForMember(dest => dest.Organizer, opt => opt.MapFrom(src => src.Organizer.Name));

        // Inbound
        CreateMap<EventCreateDto, Event>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore())
            .ForMember(dest => dest.Organizer, opt => opt.Ignore());

        CreateMap<EventUpdateDto, Event>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore())
            .ForMember(dest => dest.Organizer, opt => opt.Ignore());

        CreateMap<EventDeleteDto, Event>().ForAllMembers(opt => opt.Ignore());
    }
}
