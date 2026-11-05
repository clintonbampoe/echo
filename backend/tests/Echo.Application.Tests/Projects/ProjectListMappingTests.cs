using Echo.Application.Projects;
using Echo.Domain.Projects;

namespace Echo.Application.Tests.Projects;

[Trait("Category", "Unit")]
public class ProjectListMappingTests
{
    // The list reads the category and manager names in two small queries and does not load their rows, so mapping a
    // project must not need them.
    [Fact]
    public void ToDto_WithNames_UsesThemAndNeverTouchesTheCategoryOrManager()
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            CategoryId = 3,
            ManagerId = Guid.NewGuid(),
            Name = "Roof repair",
        };

        var dto = new ProjectMapper().ToDto(project, "Building", "Ama Mensah");

        Assert.Equal(("Building", "Ama Mensah"), (dto.CategoryName, dto.ManagerName));
        Assert.Equal((project.CategoryId, project.ManagerId), (dto.CategoryId, dto.ManagerId));
    }
}
