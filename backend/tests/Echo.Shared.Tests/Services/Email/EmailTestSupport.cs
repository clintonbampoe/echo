using Echo.Shared.Models;
using Echo.Shared.Services.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

namespace Echo.Shared.Tests.Services.Email;

internal sealed class TestContent : IEmailContent
{
    public string Subject => "Hello";
    public string HtmlBody => "<p>Hi</p>";
}

internal static class Messages
{
    public static EmailMessage To(string address = "ama@echo.test") =>
        new(address, new TestContent(), "email_verification", Guid.NewGuid(), Guid.NewGuid());
}

/// <summary>Fails the attempts the script says to fail, records when each attempt happened.</summary>
internal sealed class ScriptedEmailService(
    FakeTimeProvider time,
    Func<int, Exception?> script
) : IEmailService
{
    private int _attempts;

    public int Attempts => Volatile.Read(ref _attempts);

    public List<DateTimeOffset> AttemptTimes { get; } = [];

    public Task SendAsync(string to, IEmailContent content)
    {
        var attempt = Interlocked.Increment(ref _attempts);
        lock (AttemptTimes)
            AttemptTimes.Add(time.GetUtcNow());
        var failure = script(attempt);
        return failure is null ? Task.CompletedTask : Task.FromException(failure);
    }
}

internal sealed class CapturingLogger<T> : ILogger<T>
{
    public List<(LogLevel Level, string Message)> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        lock (Entries)
            Entries.Add((logLevel, formatter(state, exception)));
    }
}
