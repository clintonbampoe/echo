using Echo.Domain.Tithes;
using Riok.Mapperly.Abstractions;

namespace Echo.Application.Tithes;

[Mapper]
public partial class TitheMapper : ITitheMapper
{
    public TitheResponseDto ToDto(Tithe entity) =>
        new TitheResponseDto
        {
            Id = entity.Id,
            MemberId = entity.MemberId,
            MemberName = entity.Member.Person.Name,
            Amount = entity.Amount,
            ForYear = entity.ForYear,
            ForMonth = entity.ForMonth,
            PaymentMethod = entity.PaymentMethod,
            CollectionDate = entity.CollectionDate,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt,
        };

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
