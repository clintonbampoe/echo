using Echo.Domain.Assets;

namespace Echo.Core.Assets;

public interface IAssetMapper
{
    AssetResponseDto ToDto(Asset entity);
    Asset ToEntity(AssetCreateDto dto);
    List<AssetResponseDto> ToListDto(List<Asset> entities);

    List<AssetSearchResultDto> ToSearchDto(List<Asset> entities);
    void Patch(AssetUpdateDto dto, Asset entity);
}
