using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Echo.Core.Mapping.TransactionMapping;

[Mapper]
public partial class TransactionMapper : ITransactionMapper
{
    [MapperIgnoreSource(nameof(entity.Congregation))]
    [MapperIgnoreSource(nameof(entity.CongregationId))]
    [MapperIgnoreSource(nameof(entity.DeletedAt))]
    [MapProperty(nameof(Transaction.Category.Name), nameof(TransactionResponseDto.CategoryName))]
    public partial TransactionResponseDto ToDto(Transaction entity);

    [MapperIgnoreTarget(nameof(Transaction.Congregation))]
    [MapperIgnoreTarget(nameof(Transaction.CongregationId))]
    [MapperIgnoreTarget(nameof(Transaction.Id))]
    [MapperIgnoreTarget(nameof(Transaction.Category))]
    [MapperIgnoreTarget(nameof(Transaction.CreatedAt))]
    [MapperIgnoreTarget(nameof(Transaction.DeletedAt))]
    public partial Transaction ToEntity(TransactionCreateDto dto);

    public partial List<TransactionResponseDto> ToListDto(List<Transaction> entities);

    public void Patch(TransactionUpdateDto dto, Transaction entity)
    {
        if (dto.CategoryId.HasValue)
            entity.CategoryId = dto.CategoryId.Value;
        if (dto.TransactionType.HasValue)
            entity.TransactionType = dto.TransactionType.Value;
        if (dto.TransactionDate.HasValue)
            entity.TransactionDate = dto.TransactionDate.Value;
        if (dto.Amount.HasValue)
            entity.Amount = dto.Amount.Value;
        if (dto.Description != null)
            entity.Description = dto.Description;
    }
}
