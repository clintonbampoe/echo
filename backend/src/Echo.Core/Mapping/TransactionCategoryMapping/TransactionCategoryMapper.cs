using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Echo.Core.Mapping.TransactionCategoryMapping;

[Mapper]
public partial class TransactionCategoryMapper : ITransactionCategoryMapper
{
    [MapperIgnoreSource(nameof(entity.Congregation))]
    [MapperIgnoreSource(nameof(entity.CongregationId))]
    [MapperIgnoreSource(nameof(entity.CreatedAt))]
    [MapperIgnoreSource(nameof(entity.DeletedAt))]
    public partial TransactionCategoryResponseDto ToDto(TransactionCategory entity);

    [MapperIgnoreTarget(nameof(TransactionCategory.Congregation))]
    [MapperIgnoreTarget(nameof(TransactionCategory.CongregationId))]
    [MapperIgnoreTarget(nameof(TransactionCategory.Id))]
    [MapperIgnoreTarget(nameof(TransactionCategory.CreatedAt))]
    [MapperIgnoreTarget(nameof(TransactionCategory.DeletedAt))]
    public partial TransactionCategory ToEntity(TransactionCategoryCreateDto dto);

    public void Patch(TransactionCategoryUpdateDto dto, TransactionCategory entity)
    {
        if (dto.Name != null) entity.Name = dto.Name;
        if (dto.CategoryType.HasValue) entity.CategoryType = dto.CategoryType.Value;
    }
}
