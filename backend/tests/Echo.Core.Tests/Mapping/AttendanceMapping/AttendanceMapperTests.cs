using Echo.Core.Mapping.AttendanceMapping;
using Echo.Core.Tests.TestData.Factories;

namespace Echo.Core.Tests.Mapping.AttendanceMapping;

public class AttendanceMapperTests
{
    private readonly IAttendanceMapper _mapper = new AttendanceMapper();

    [Fact]
    public void ToDto_ShouldMapCorrectly()
    {
        // Arrange
        var entity = AttendanceFactory.NewEntity();

        // Act
        var result = _mapper.ToDto(entity);

        // Assert
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.AttendanceContext.Name, result.AttendanceContextName);
        Assert.Equal(entity.AttendanceContext.AttendanceType.Name, result.AttendanceTypeName);
        Assert.Equal(entity.GuestName, result.GuestName);
        Assert.Equal(entity.ForDate, result.ForDate);
    }
}
