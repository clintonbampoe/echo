using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Dtos;

public record OrganizationCreateDto : ICreateDto<Organization>
{
    public Guid CongregationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public record OrganizationResponseDto : IResponseDto<Organization>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public record OrganizationListResponseDto : IListResponseDto<Organization>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Name { get; init; } = string.Empty;
}

public record OrganizationUpdateDto : IUpdateDto<Organization>
{
    public Guid CongregationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public record OrganizationDeleteDto : ISoftDeleteDto<Organization>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
}
