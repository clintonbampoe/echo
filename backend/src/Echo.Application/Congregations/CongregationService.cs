using Echo.Shared.HttpResults;

namespace Echo.Application.Congregations;

public class CongregationService(
    CongregationRepository repository,
    ICongregationMapper mapper,
    ApplicationInstrumentation instrumentation
)
{
    public async Task<IOperationResult> Get(Guid congregationId, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.congregation.get");

        var entity = await repository.GetById(congregationId, ct);
        if (entity is null)
            return new NotFoundResult(congregationId.ToString());

        return new SuccessResult<CongregationResponseDto>(mapper.ToDto(entity));
    }
}
