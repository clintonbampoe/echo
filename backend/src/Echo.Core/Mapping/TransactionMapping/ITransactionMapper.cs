using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.TransactionMapping;

public interface ITransactionMapper
{
    TransactionResponseDto ToDto(Transaction entity);
    Transaction ToEntity(TransactionCreateDto dto);
    void Patch(TransactionUpdateDto dto, Transaction entity);
}
