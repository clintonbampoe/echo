using AutoMapper;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Common.Mapping;

public class EventAttendanceProfile : Profile
{
    public EventAttendanceProfile()
    {
        // Outbound only
        CreateMap<EventAttendance, EventAttendanceResponseDto>()
            .ForMember(dest => dest.Member, opt => opt.MapFrom(src => src.Member.Name))
            .ForMember(dest => dest.Event, opt => opt.MapFrom(src => src.Event.Name));

        CreateMap<EventAttendance, EventAttendanceListResponseDto>()
            .ForMember(dest => dest.Member, opt => opt.MapFrom(src => src.Member.Name))
            .ForMember(dest => dest.Event, opt => opt.MapFrom(src => src.Event.Name));

        // Inbound
        CreateMap<EventAttendanceCreateDto, EventAttendance>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Member, opt => opt.Ignore())
            .ForMember(dest => dest.Event, opt => opt.Ignore());

        CreateMap<EventAttendanceUpdateDto, EventAttendance>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Member, opt => opt.Ignore())
            .ForMember(dest => dest.Event, opt => opt.Ignore());

        CreateMap<EventAttendanceDeleteDto, EventAttendance>().ForAllMembers(opt => opt.Ignore());
    }
}
