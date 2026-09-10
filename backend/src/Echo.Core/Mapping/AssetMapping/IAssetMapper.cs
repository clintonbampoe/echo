using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.AssetMapping;

public interface IAssetMapper
{
    AssetResponseDto ToDto(Asset entity);
    Asset ToEntity(AssetCreateDto dto);
    List<AssetResponseDto> ToListDto(List<Asset> entities);

    List<AssetSearchResultDto> ToSearchDto(List<Asset> entities);
    void Patch(AssetUpdateDto dto, Asset entity);
}
