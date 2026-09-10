using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.TransactionMapping;

public interface ITransactionMapper
{
    TransactionResponseDto ToDto(Transaction entity);
    Transaction ToEntity(TransactionCreateDto dto);
    List<TransactionResponseDto> ToListDto(List<Transaction> entities);

    void Patch(TransactionUpdateDto dto, Transaction entity);
}
