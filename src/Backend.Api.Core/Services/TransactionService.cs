using AutoMapper;
using Backend.Api.Core.Common.HttpResults;
using Backend.Api.Core.Common.HttpResults.Interfaces;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories;
using Backend.Api.Core.Services.Base;

namespace Backend.Api.Core.Services;

public class TransactionService(TransactionRepository repository, IMapper mapper)
    : ServiceBase<Transaction>(repository, mapper)
{
    private readonly TransactionRepository _repository = repository;

    public async Task<IOperationResult> GetSummaryAsync()
    {
        var streams = await _repository.GetStreamsAsync();

        var incomeData = streams.Where(s => s.Type == Enums.TransactionType.Income).ToList();
        var expenditureData = streams
            .Where(s => s.Type == Enums.TransactionType.Expenditure)
            .ToList();

        var totalIncome = incomeData.Sum(x => x.Amount);
        var totalExpenditure = expenditureData.Sum(x => x.Amount);

        var summary = new TransactionSummaryDto
        {
            TotalIncome = totalIncome,
            TotalExpenditure = totalExpenditure,
            NetBalance = totalIncome - totalExpenditure,

            IncomeStreams = incomeData
                .Select(x => new TransactionIncomeStreamDto
                {
                    Category = x.Category,
                    Amount = x.Amount,
                    PercentageOfTotal = totalIncome == 0 ? 0 : x.Amount / totalIncome * 100,
                })
                .ToList(),

            ExpenditureStreams = expenditureData
                .Select(x => new TransactionExpenditureStreamDto
                {
                    Category = x.Category,
                    Amount = x.Amount,
                    PercentageOfTotal =
                        totalExpenditure == 0 ? 0 : x.Amount / totalExpenditure * 100,
                })
                .ToList(),
        };

        return new SuccessResult<TransactionSummaryDto>(summary);
    }
}
