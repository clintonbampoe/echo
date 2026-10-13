using Echo.Application.Events;

namespace Echo.Application.Tests.Events;

[Trait("Category", "Unit")]
public class EventCapacityTests
{
    [Theory]
    [InlineData(null, 0, false)] // no capacity: unlimited
    [InlineData(null, 5000, false)]
    [InlineData(60, 59, false)]
    [InlineData(60, 60, true)] // exactly full
    [InlineData(60, 75, true)] // capacity lowered below the registrations: still full
    [InlineData(0, 0, true)]
    public void IsFull_WhenTheRegistrationsHaveReachedTheCapacity(int? capacity, int registered, bool expected)
    {
        Assert.Equal(expected, EventCapacity.IsFull(capacity, registered));
    }
}
