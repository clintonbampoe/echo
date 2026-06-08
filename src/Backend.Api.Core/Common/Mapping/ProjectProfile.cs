using AutoMapper;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Common.Mapping;

public class ProjectProfile : Profile
{
    public ProjectProfile()
    {
        // Outbound only
        CreateMap<Project, ProjectResponseDto>()
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.Manager, opt => opt.MapFrom(src => src.Manager.Name));

        CreateMap<Project, ProjectListResponseDto>()
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.Manager, opt => opt.MapFrom(src => src.Manager.Name));

        // Inbound
        CreateMap<ProjectCreateDto, Project>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Manager, opt => opt.Ignore());

        CreateMap<ProjectUpdateDto, Project>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Manager, opt => opt.Ignore());

        CreateMap<ProjectDeleteDto, Project>().ForAllMembers(opt => opt.Ignore());
    }
}
