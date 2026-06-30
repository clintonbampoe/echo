using AutoMapper;
using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping;

public class EventAttendanceProfile : Profile
{
    public EventAttendanceProfile()
    {
        CreateMap<EventAttendanceCreateDto, EventAttendance>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CongregationId, opt => opt.Ignore())
            .ForMember(dest => dest.Congregation, opt => opt.Ignore())
            .ForMember(dest => dest.Member, opt => opt.Ignore())
            .ForMember(dest => dest.Event, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        CreateMap<EventAttendanceUpdateDto, EventAttendance>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CongregationId, opt => opt.Ignore())
            .ForMember(dest => dest.Congregation, opt => opt.Ignore())
            .ForMember(dest => dest.Member, opt => opt.Ignore())
            .ForMember(dest => dest.Event, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());
    }
}
