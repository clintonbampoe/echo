using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Echo.Core.Mapping.TitheMapping;

[Mapper]
public partial class TitheMapper : ITitheMapper
{
    [MapperIgnoreSource(nameof(entity.Congregation))]
    [MapperIgnoreSource(nameof(entity.CongregationId))]
    [MapperIgnoreSource(nameof(entity.DeletedAt))]
    [MapProperty(nameof(Tithe.Member.Name), nameof(TitheResponseDto.MemberName))]
    public partial TitheResponseDto ToDto(Tithe entity);

    [MapperIgnoreTarget(nameof(Tithe.Congregation))]
    [MapperIgnoreTarget(nameof(Tithe.CongregationId))]
    [MapperIgnoreTarget(nameof(Tithe.Id))]
    [MapperIgnoreTarget(nameof(Tithe.Member))]
    [MapperIgnoreTarget(nameof(Tithe.CreatedAt))]
    [MapperIgnoreTarget(nameof(Tithe.DeletedAt))]
    public partial Tithe ToEntity(TitheCreateDto dto);

    public partial List<TitheResponseDto> ToListDto(List<Tithe> entities);

    public void Patch(TitheUpdateDto dto, Tithe entity)
    {
        if (dto.MemberId.HasValue)
            entity.MemberId = dto.MemberId.Value;
        if (dto.Amount.HasValue)
            entity.Amount = dto.Amount.Value;
        if (dto.ForYear.HasValue)
            entity.ForYear = dto.ForYear.Value;
        if (dto.ForMonth.HasValue)
            entity.ForMonth = dto.ForMonth.Value;
        if (dto.PaymentMethod.HasValue)
            entity.PaymentMethod = dto.PaymentMethod.Value;
        if (dto.CollectionDate.HasValue)
            entity.CollectionDate = dto.CollectionDate.Value;
        if (dto.Description != null)
            entity.Description = dto.Description;
    }
}
