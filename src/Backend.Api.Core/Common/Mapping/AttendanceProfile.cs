using AutoMapper;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Common.Mapping;

public class AttendanceProfile : Profile
{
    public AttendanceProfile()
    {
        // Outbound only
        CreateMap<AttendanceRecord, AttendanceResponseDto>()
            .ForMember(dest => dest.Member, opt => opt.MapFrom(src => src.Member.Name));

        CreateMap<AttendanceRecord, AttendanceListResponseDto>()
            .ForMember(dest => dest.Member, opt => opt.MapFrom(src => src.Member.Name));

        // Inbound
        CreateMap<AttendanceCreateDto, AttendanceRecord>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Member, opt => opt.Ignore());

        CreateMap<AttendanceUpdateDto, AttendanceRecord>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Member, opt => opt.Ignore());

        CreateMap<AttendanceDeleteDto, AttendanceRecord>().ForAllMembers(opt => opt.Ignore());
    }
}
