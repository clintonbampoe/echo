using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Echo.Domain.Enums;

namespace Echo.Core.Tests.TestData.Factories;

public static class ProjectContributionFactory
{
    public static ProjectContribution NewEntity()
    {
        return new ProjectContribution()
        {
            Id = Constants.DefaultGuid,
            CongregationId = Constants.DefaultGuid,
            ProjectId = Constants.DefaultGuid,
            Project = ProjectFactory.NewEntity(),
            Amount = 100.0m,
            DateContributed = Constants.DefaultDateOnly,
            PaymentMethod = PaymentMethod.Cash,
            Description = "Contr-01",
            CreatedAt = Constants.DefaultDateTime,
            DeletedAt = null,
        };
    }

    public static ProjectContributionCreateDto NewCreateDto()
    {
        return new ProjectContributionCreateDto()
        {
            ProjectId = Constants.DefaultGuid,
            Amount = 100.0m,
            DateContributed = Constants.DefaultDateOnly,
            PaymentMethod = PaymentMethod.Cash,
            Description = "Contr-01",
        };
    }

    public static ProjectContributionUpdateDto NewUpdateDto()
    {
        return new ProjectContributionUpdateDto()
        {
            Amount = 100.0m,
            DateContributed = Constants.DefaultDateOnly,
            PaymentMethod = PaymentMethod.Cash,
            Description = "Contr-01",
        };
    }

    public static ProjectContributionUpdateDto NewUpdateDtoWithNullValues()
    {
        return new ProjectContributionUpdateDto()
        {
            Amount = null,
            DateContributed = null,
            PaymentMethod = null,
            Description = null,
        };
    }
}
