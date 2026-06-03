using AutoMapper;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Entities.Dtos;

namespace Backend.Api.Core.Common.Mapping;

public class ProjectProfile : Profile
{
    public ProjectProfile()
    {
        CreateMap<Project, ProjectResponseDto>().ReverseMap();
        CreateMap<Project, ProjectListResponseDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.ManagerName, opt => opt.MapFrom(src => src.Manager.Name))
            .ReverseMap()
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Manager, opt => opt.Ignore());

        CreateMap<Project, ProjectCreateDto>()
            .ReverseMap()
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            
        CreateMap<Project, ProjectUpdateDto>().ReverseMap();
        CreateMap<Project, ProjectDeleteDto>().ReverseMap();
    }
}
