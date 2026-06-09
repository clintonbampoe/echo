using AutoMapper;
using Backend.Api.Core.Common.HttpResults;
using Backend.Api.Core.Common.HttpResults.Interfaces;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories;
using Backend.Api.Core.Services.Base;

namespace Backend.Api.Core.Services;

public class TitheService(TitheRepository repository, IMapper mapper)
    : ServiceBase<Tithe>(repository, mapper)
{
    private readonly TitheRepository _repository = repository;

    public async Task<IOperationResult> GetSummaryAsync(
        Guid congregationId,
        int year,
        CancellationToken ct
    )
    {
        var aggregatedMonthlySummary = await _repository.GetAggregatedSummaryByMonth(
            congregationId,
            year,
            ct
        );

        var summary = new TitheMonthlySummaryDto { Months = aggregatedMonthlySummary, Year = year };
        return new SuccessResult<TitheMonthlySummaryDto>(summary);
    }
}
