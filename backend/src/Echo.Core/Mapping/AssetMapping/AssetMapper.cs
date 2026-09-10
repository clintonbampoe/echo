using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Echo.Core.Mapping.AssetMapping;

[Mapper]
public partial class AssetMapper : IAssetMapper
{
    [MapperIgnoreSource(nameof(entity.Congregation))]
    [MapperIgnoreSource(nameof(entity.CongregationId))]
    [MapperIgnoreSource(nameof(entity.DeletedAt))]
    [MapProperty(
        [nameof(Asset.Category), nameof(entity.Category.Name)],
        nameof(AssetResponseDto.CategoryName)
    )]
    public partial AssetResponseDto ToDto(Asset entity);

    [MapperIgnoreTarget(nameof(Asset.Congregation))]
    [MapperIgnoreTarget(nameof(Asset.CongregationId))]
    [MapperIgnoreTarget(nameof(Asset.Id))]
    [MapperIgnoreTarget(nameof(Asset.Category))]
    [MapperIgnoreTarget(nameof(Asset.CreatedAt))]
    [MapperIgnoreTarget(nameof(Asset.DeletedAt))]
    public partial Asset ToEntity(AssetCreateDto dto);

    public partial List<AssetResponseDto> ToListDto(List<Asset> entities);

    public List<AssetSearchResultDto> ToSearchDto(List<Asset> entities)
    {
        var res = entities
            .Select(e => new AssetSearchResultDto() { Id = e.Id, Name = e.Name })
            .ToList();
        return res;
    }

    public void Patch(AssetUpdateDto dto, Asset entity)
    {
        if (dto.CategoryId.HasValue)
            entity.CategoryId = dto.CategoryId.Value;
        if (dto.Name != null)
            entity.Name = dto.Name;
        if (dto.SerialNumber != null)
            entity.SerialNumber = dto.SerialNumber;
        if (dto.PurchaseDate.HasValue)
            entity.PurchaseDate = dto.PurchaseDate.Value;
        if (dto.PurchaseCost.HasValue)
            entity.PurchaseCost = dto.PurchaseCost.Value;
        if (dto.CurrentValue.HasValue)
            entity.CurrentValue = dto.CurrentValue.Value;
        if (dto.Status.HasValue)
            entity.Status = dto.Status.Value;
        if (dto.Description != null)
            entity.Description = dto.Description;
    }
}
