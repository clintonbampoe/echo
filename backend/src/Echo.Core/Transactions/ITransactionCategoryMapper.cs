using Echo.Domain.Transactions;

namespace Echo.Core.Transactions;

public interface ITransactionCategoryMapper
{
    TransactionCategoryResponseDto ToDto(TransactionCategory entity);
    TransactionCategory ToEntity(TransactionCategoryCreateDto dto);
    List<TransactionCategoryResponseDto> ToListDto(List<TransactionCategory> entities);

    List<TransactionCategorySearchResponseDto> ToSearchDto(List<TransactionCategory> entities);
    void Patch(TransactionCategoryUpdateDto dto, TransactionCategory entity);
}
