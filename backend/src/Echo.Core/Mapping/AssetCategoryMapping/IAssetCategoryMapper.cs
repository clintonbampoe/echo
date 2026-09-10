using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.AssetCategoryMapping;

public interface IAssetCategoryMapper
{
    AssetCategoryResponseDto ToDto(AssetCategory entity);
    AssetCategory ToEntity(AssetCategoryCreateDto dto);
    List<AssetCategoryResponseDto> ToListDto(List<AssetCategory> entities);

    List<AssetCategorySearchResultDto> ToSearchDto(List<AssetCategory> entities);
    void Patch(AssetCategoryUpdateDto dto, AssetCategory entity);
}
