using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Echo.Domain.Enums;

namespace Echo.Core.Tests.TestData.Factories;

public static class ProjectFactory
{
    public static Project NewEntity()
    {
        return new Project()
        {
            Id = Constants.DefaultGuid,
            CongregationId = Constants.DefaultGuid,
            CategoryId = Constants.DefaultInt,
            Category = ProjectCategoryFactory.NewEntity(),
            ManagerId = Constants.DefaultGuid,
            Manager = MemberFactory.NewEntity(),
            Name = "Project-01",
            TargetAmount = 10000.0m,
            Status = ProjectStatus.Planning,
            StartDate = Constants.DefaultDateOnly,
            EndDate = Constants.DefaultDateOnly,
            Description = "ProjectDesc-01",
            CreatedAt = Constants.DefaultDateTime,
            DeletedAt = null,
        };
    }

    public static ProjectCreateDto NewCreateDto()
    {
        return new ProjectCreateDto()
        {
            CategoryId = Constants.DefaultInt,
            ManagerId = Constants.DefaultGuid,
            Name = "Project-01",
            TargetAmount = 10000.0m,
            Status = ProjectStatus.Planning,
            StartDate = Constants.DefaultDateOnly,
            EndDate = Constants.DefaultDateOnly,
            Description = "ProjectDesc-01",
        };
    }

    public static ProjectUpdateDto NewUpdateDto()
    {
        return new ProjectUpdateDto()
        {
            CategoryId = Constants.DefaultInt,
            ManagerId = Constants.DefaultGuid,
            Name = "Project-01",
            TargetAmount = 10000.0m,
            Status = ProjectStatus.Planning,
            StartDate = Constants.DefaultDateOnly,
            EndDate = Constants.DefaultDateOnly,
            Description = "ProjectDesc-01",
        };
    }

    public static ProjectUpdateDto NewUpdateDtoWithNullValues()
    {
        return new ProjectUpdateDto()
        {
            CategoryId = null,
            ManagerId = null,
            Name = null,
            TargetAmount = null,
            Status = null,
            StartDate = null,
            EndDate = null,
            Description = null,
        };
    }
}
