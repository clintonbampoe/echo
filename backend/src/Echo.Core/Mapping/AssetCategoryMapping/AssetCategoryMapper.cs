using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Echo.Core.Mapping.AssetCategoryMapping;

[Mapper]
public partial class AssetCategoryMapper : IAssetCategoryMapper
{
    [MapperIgnoreSource(nameof(entity.Congregation))]
    [MapperIgnoreSource(nameof(entity.CongregationId))]
    [MapperIgnoreSource(nameof(entity.CreatedAt))]
    [MapperIgnoreSource(nameof(entity.DeletedAt))]
    public partial AssetCategoryResponseDto ToDto(AssetCategory entity);

    [MapperIgnoreTarget(nameof(AssetCategory.CreatedAt))]
    [MapperIgnoreTarget(nameof(AssetCategory.DeletedAt))]
    [MapperIgnoreTarget(nameof(AssetCategory.Congregation))]
    [MapperIgnoreTarget(nameof(AssetCategory.CongregationId))]
    [MapperIgnoreTarget(nameof(AssetCategory.Id))]
    public partial AssetCategory ToEntity(AssetCategoryCreateDto dto);

    public partial List<AssetCategoryResponseDto> ToListDto(List<AssetCategory> entities);

    public List<AssetCategorySearchResultDto> ToSearchDto(List<AssetCategory> entities)
    {
        var res = entities
            .Select(a => new AssetCategorySearchResultDto() { Id = a.Id, Name = a.Name })
            .ToList();
        return res;
    }

    public void Patch(AssetCategoryUpdateDto dto, AssetCategory entity)
    {
        if (dto.Name != null)
            entity.Name = dto.Name;
    }
}
