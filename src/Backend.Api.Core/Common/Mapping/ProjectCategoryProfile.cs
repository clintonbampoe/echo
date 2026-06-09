using AutoMapper;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Common.Mapping;

public class ProjectCategoryProfile : Profile
{
    public ProjectCategoryProfile()
    {
        CreateMap<ProjectCategory, ProjectCategoryResponseDto>();
        CreateMap<ProjectCategory, ProjectCategoryListResponseDto>();

        CreateMap<ProjectCategoryCreateDto, ProjectCategory>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        CreateMap<ProjectCategoryUpdateDto, ProjectCategory>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        CreateMap<ProjectCategoryDeleteDto, ProjectCategory>().ForAllMembers(opt => opt.Ignore());
    }
}
