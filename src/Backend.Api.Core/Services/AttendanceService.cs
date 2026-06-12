using AutoMapper;
using Backend.Api.Core.Common.HttpResults;
using Backend.Api.Core.Common.HttpResults.Interfaces;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Enums;
using Backend.Api.Core.Repositories;
using Backend.Api.Core.Services.Base;

namespace Backend.Api.Core.Services;

public class AttendanceService(AttendanceRepository repository, IMapper mapper)
    : ServiceBase<AttendanceRecord>(repository, mapper)
{
    private readonly AttendanceRepository _repository = repository;

    public async Task<IOperationResult> GetSummaryAsync(
        Guid congregationId,
        DateOnly forDate,
        ChurchServiceType churchServiceType,
        CancellationToken ct
    )
    {
        var summary = await _repository.GetSummaryAsync(
            congregationId,
            forDate,
            churchServiceType,
            ct
        );

        return new SuccessResult<AttendanceSummaryDto>(summary);
    }
}
