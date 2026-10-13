using Echo.Data;
using Echo.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Tests.Events;

// EventRegistrationRepository.TryCreateWithinCapacity locks the event row with hand-written SQL:
//   SELECT 1 FROM "Events" WHERE "Id" = ... FOR UPDATE
// If the table or key column were ever renamed, that SQL would only fail at runtime, on events
// that have a capacity. This pins the names it assumes so a rename fails the build instead.
[Trait("Category", "Unit")]
public class EventRegistrationRawSqlTests
{
    [Fact]
    public void TheEventTableAndKeyColumn_AreTheOnesTheLockStatementNames()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=unused;Username=x;Password=x")
            .Options;
        using var context = new AppDbContext(options);

        var eventType = context.Model.FindEntityType(typeof(Event))!;

        Assert.Equal("Events", eventType.GetTableName());
        Assert.Null(eventType.GetSchema());
        Assert.Equal("Id", eventType.FindProperty(nameof(Event.Id))!.GetColumnName());
    }
}
