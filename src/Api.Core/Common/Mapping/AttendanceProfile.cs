using Api.Core.Dtos;
using Api.Core.Entities;
using AutoMapper;

namespace Api.Core.Common.Mapping;

public class AttendanceProfile : Profile
{
    public AttendanceProfile()
    {
        CreateMap<AttendanceCreateDto, Attendance>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CongregationId, opt => opt.Ignore())
            .ForMember(dest => dest.Congregation, opt => opt.Ignore())
            .ForMember(dest => dest.AttendanceContext, opt => opt.Ignore())
            .ForMember(dest => dest.Member, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        CreateMap<AttendanceUpdateDto, Attendance>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CongregationId, opt => opt.Ignore())
            .ForMember(dest => dest.Congregation, opt => opt.Ignore())
            .ForMember(dest => dest.AttendanceContext, opt => opt.Ignore())
            .ForMember(dest => dest.Member, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());
    }
}
