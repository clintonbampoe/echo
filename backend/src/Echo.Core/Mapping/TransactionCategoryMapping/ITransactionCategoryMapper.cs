using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.TransactionCategoryMapping;

public interface ITransactionCategoryMapper
{
    TransactionCategoryResponseDto ToDto(TransactionCategory entity);
    TransactionCategory ToEntity(TransactionCategoryCreateDto dto);
    List<TransactionCategoryResponseDto> ToListDto(List<TransactionCategory> entities);

    List<TransactionCategorySearchResponseDto> ToSearchDto(List<TransactionCategory> entities);
    void Patch(TransactionCategoryUpdateDto dto, TransactionCategory entity);
}
