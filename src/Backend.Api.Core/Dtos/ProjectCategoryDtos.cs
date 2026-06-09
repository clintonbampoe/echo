using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Dtos;

public record ProjectCategoryCreateDto : ICreateDto<ProjectCategory>
{
    public Guid CongregationId { get; init; }
    public string Name { get; init; } = string.Empty;
}

public record ProjectCategoryResponseDto : IResponseDto<ProjectCategory>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Name { get; init; } = string.Empty;
}

public record ProjectCategoryListResponseDto : IListResponseDto<ProjectCategory>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Name { get; init; } = string.Empty;
}

public record ProjectCategoryUpdateDto : IUpdateDto<ProjectCategory>
{
    public Guid CongregationId { get; init; }
    public string Name { get; init; } = string.Empty;
}

public record ProjectCategoryDeleteDto : ISoftDeleteDto<ProjectCategory>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
}
