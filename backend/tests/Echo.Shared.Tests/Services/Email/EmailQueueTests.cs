using Echo.Shared.Services.Email;
using Microsoft.Extensions.Logging.Abstractions;

namespace Echo.Shared.Tests.Services.Email;

[Trait("Category", "Unit")]
public class EmailQueueTests
{
    [Fact]
    public void TryEnqueue_WhenFull_ReturnsFalseAndKeepsTheFirstMessages()
    {
        var queue = new EmailQueue(NullLogger<EmailQueue>.Instance);

        for (var i = 0; i < EmailQueue.Capacity; i++)
            Assert.True(queue.TryEnqueue(Messages.To($"user{i}@echo.test")));

        Assert.False(queue.TryEnqueue(Messages.To("overflow@echo.test")));
        Assert.Equal(EmailQueue.Capacity, queue.Reader.Count);
        Assert.True(queue.Reader.TryRead(out var first));
        Assert.Equal("user0@echo.test", first.To);
    }

    [Fact]
    public void TryEnqueue_WhenFull_LogsAWarning()
    {
        var logger = new CapturingLogger<EmailQueue>();
        var queue = new EmailQueue(logger);
        for (var i = 0; i < EmailQueue.Capacity; i++)
            queue.TryEnqueue(Messages.To());

        queue.TryEnqueue(Messages.To());

        Assert.Contains(logger.Entries, e => e.Level == Microsoft.Extensions.Logging.LogLevel.Warning);
    }
}
