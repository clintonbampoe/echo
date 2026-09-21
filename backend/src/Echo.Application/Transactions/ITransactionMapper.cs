using Echo.Domain.Transactions;

namespace Echo.Application.Transactions;

public interface ITransactionMapper
{
    TransactionResponseDto ToDto(Transaction entity);
    Transaction ToEntity(TransactionCreateDto dto);
    List<TransactionResponseDto> ToListDto(List<Transaction> entities);

    void Patch(TransactionUpdateDto dto, Transaction entity);
}
