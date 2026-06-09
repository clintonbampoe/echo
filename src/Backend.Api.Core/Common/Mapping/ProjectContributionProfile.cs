using AutoMapper;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Common.Mapping;

public class ProjectContributionProfile : Profile
{
    public ProjectContributionProfile()
    {
        CreateMap<ProjectContribution, ProjectContributionResponseDto>()
            .ForMember(dest => dest.Project, opt => opt.MapFrom(src => src.Project.Name));

        CreateMap<ProjectContribution, ProjectContributionListResponseDto>()
            .ForMember(dest => dest.Project, opt => opt.MapFrom(src => src.Project.Name));

        CreateMap<ProjectContributionCreateDto, ProjectContribution>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Project, opt => opt.Ignore());

        CreateMap<ProjectContributionUpdateDto, ProjectContribution>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Project, opt => opt.Ignore());

        CreateMap<ProjectContributionDeleteDto, ProjectContribution>()
            .ForAllMembers(opt => opt.Ignore());
    }
}
