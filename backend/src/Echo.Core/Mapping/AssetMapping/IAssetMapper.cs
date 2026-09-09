using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.AssetMapping;

public interface IAssetMapper
{
    AssetResponseDto ToDto(Asset entity);
    Asset ToEntity(AssetCreateDto dto);
    void Patch(AssetUpdateDto dto, Asset entity);
}
