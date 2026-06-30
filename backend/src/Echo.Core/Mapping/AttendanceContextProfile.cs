using AutoMapper;
using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping;

public class AttendanceContextProfile : Profile
{
    public AttendanceContextProfile()
    {
        CreateMap<AttendanceContextCreateDto, AttendanceContext>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CongregationId, opt => opt.Ignore())
            .ForMember(dest => dest.Congregation, opt => opt.Ignore())
            .ForMember(dest => dest.AttendanceType, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        CreateMap<AttendanceContextUpdateDto, AttendanceContext>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CongregationId, opt => opt.Ignore())
            .ForMember(dest => dest.Congregation, opt => opt.Ignore())
            .ForMember(dest => dest.AttendanceType, opt => opt.Ignore())
            .ForMember(dest => dest.AttendanceTypeId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());
    }
}
