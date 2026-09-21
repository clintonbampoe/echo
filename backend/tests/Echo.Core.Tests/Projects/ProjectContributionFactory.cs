using Echo.Core.Projects;
using Echo.Domain.Projects;
using Echo.Domain.Transactions;

namespace Echo.Core.Tests.Projects;

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
