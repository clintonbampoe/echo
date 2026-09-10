using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Echo.Core.Mapping.ProjectContributionMapping;

[Mapper]
public partial class ProjectContributionMapper : IProjectContributionMapper
{
    [MapperIgnoreSource(nameof(entity.Congregation))]
    [MapperIgnoreSource(nameof(entity.CongregationId))]
    [MapperIgnoreSource(nameof(entity.DeletedAt))]
    [MapProperty(
        nameof(ProjectContribution.Project.Name),
        nameof(ProjectContributionResponseDto.ProjectName)
    )]
    public partial ProjectContributionResponseDto ToDto(ProjectContribution entity);

    [MapperIgnoreTarget(nameof(ProjectContribution.Congregation))]
    [MapperIgnoreTarget(nameof(ProjectContribution.CongregationId))]
    [MapperIgnoreTarget(nameof(ProjectContribution.Id))]
    [MapperIgnoreTarget(nameof(ProjectContribution.Project))]
    [MapperIgnoreTarget(nameof(ProjectContribution.CreatedAt))]
    [MapperIgnoreTarget(nameof(ProjectContribution.DeletedAt))]
    public partial ProjectContribution ToEntity(ProjectContributionCreateDto dto);

    public partial List<ProjectContributionResponseDto> ToListDto(
        List<ProjectContribution> entities
    );

    public void Patch(ProjectContributionUpdateDto dto, ProjectContribution entity)
    {
        if (dto.Amount.HasValue)
            entity.Amount = dto.Amount.Value;
        if (dto.DateContributed.HasValue)
            entity.DateContributed = dto.DateContributed.Value;
        if (dto.PaymentMethod.HasValue)
            entity.PaymentMethod = dto.PaymentMethod.Value;
        if (dto.Description != null)
            entity.Description = dto.Description;
    }
}
