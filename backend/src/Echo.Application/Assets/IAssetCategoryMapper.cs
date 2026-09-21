using Echo.Domain.Assets;

namespace Echo.Application.Assets;

public interface IAssetCategoryMapper
{
    AssetCategoryResponseDto ToDto(AssetCategory entity);
    AssetCategory ToEntity(AssetCategoryCreateDto dto);
    List<AssetCategoryResponseDto> ToListDto(List<AssetCategory> entities);

    List<AssetCategorySearchResultDto> ToSearchDto(List<AssetCategory> entities);
    void Patch(AssetCategoryUpdateDto dto, AssetCategory entity);
}
