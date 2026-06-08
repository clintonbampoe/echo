using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Dtos;

public record AssetCategoryCreateDto : ICreateDto<AssetCategory>
{
    public Guid CongregationId { get; init; }
    public string Name { get; init; } = string.Empty;
}

public record AssetCategoryResponseDto : IResponseDto<AssetCategory>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Name { get; init; } = string.Empty;
}

public record AssetCategoryListResponseDto : IListResponseDto<AssetCategory>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Name { get; init; } = string.Empty;
}

public record AssetCategoryUpdateDto : IUpdateDto<AssetCategory>
{
    public Guid CongregationId { get; init; }
    public string Name { get; init; } = string.Empty;
}

public record AssetCategoryDeleteDto : ISoftDeleteDto<AssetCategory>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
}
