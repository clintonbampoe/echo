using Echo.Shared.Services.Email;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace Echo.Shared.Tests.Services.Email;

[Trait("Category", "Unit")]
public class EmailDeliveryWorkerTests
{
    private static readonly Exception Transient = new HttpRequestException("resend is down");
    private static readonly Exception Permanent = new InvalidOperationException("bad address");

    private sealed record Rig(
        EmailQueue Queue,
        ScriptedEmailService Email,
        FakeTimeProvider Time,
        CapturingLogger<EmailDeliveryWorker> Logger,
        EmailDeliveryWorker Worker
    );

    private static Rig Build(Func<int, Exception?> script)
    {
        var time = new FakeTimeProvider();
        var email = new ScriptedEmailService(time, script);
        var services = new ServiceCollection().AddSingleton<IEmailService>(email).BuildServiceProvider();
        var queue = new EmailQueue(NullLogger<EmailQueue>.Instance);
        var logger = new CapturingLogger<EmailDeliveryWorker>();
        var worker = new EmailDeliveryWorker(
            queue,
            services.GetRequiredService<IServiceScopeFactory>(),
            time,
            logger
        );
        return new Rig(queue, email, time, logger, worker);
    }

    // Moves fake time forward in small steps until the condition holds. The worker registers its
    // back-off timer a moment after a failed attempt, so a single big jump could miss it.
    private static async Task Pump(Rig rig, Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (!condition())
        {
            Assert.True(DateTime.UtcNow < deadline, "timed out waiting for the worker");
            rig.Time.Advance(TimeSpan.FromSeconds(1));
            await Task.Delay(5);
        }
    }

    private static async Task WaitFor(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (!condition())
        {
            Assert.True(DateTime.UtcNow < deadline, "timed out waiting for the worker");
            await Task.Delay(5);
        }
    }

    [Fact]
    public async Task Send_Succeeds_FirstAttempt_SendsOnce()
    {
        var rig = Build(_ => null);
        await rig.Worker.StartAsync(TestContext.Current.CancellationToken);

        rig.Queue.TryEnqueue(Messages.To());
        await WaitFor(() => rig.Email.Attempts == 1);
        await rig.Worker.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, rig.Email.Attempts);
        Assert.DoesNotContain(rig.Logger.Entries, e => e.Level >= LogLevel.Error);
    }

    [Fact]
    public async Task Send_TransientFailureThenSuccess_RetriesWithBackOff()
    {
        var rig = Build(attempt => attempt < 3 ? Transient : null);
        await rig.Worker.StartAsync(TestContext.Current.CancellationToken);

        rig.Queue.TryEnqueue(Messages.To());
        await WaitFor(() => rig.Email.Attempts == 1);
        await Task.Delay(50, TestContext.Current.CancellationToken);
        Assert.Equal(1, rig.Email.Attempts); // still backing off, no time has passed

        await Pump(rig, () => rig.Email.Attempts == 3);
        await rig.Worker.StopAsync(TestContext.Current.CancellationToken);

        var times = rig.Email.AttemptTimes;
        Assert.True(times[1] - times[0] >= TimeSpan.FromSeconds(2));
        Assert.True(times[2] - times[1] >= TimeSpan.FromSeconds(4));
    }

    [Fact]
    public async Task Send_PermanentFailure_TriesOnce_AndLogsAnError()
    {
        var rig = Build(_ => Permanent);
        await rig.Worker.StartAsync(TestContext.Current.CancellationToken);

        rig.Queue.TryEnqueue(Messages.To());
        await WaitFor(() => rig.Logger.Entries.Any(e => e.Level == LogLevel.Error));
        rig.Time.Advance(TimeSpan.FromMinutes(1));
        await Task.Delay(50, TestContext.Current.CancellationToken);
        await rig.Worker.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, rig.Email.Attempts);
    }

    [Fact]
    public async Task Send_AlwaysTransient_StopsAfterFourAttemptsAndLogsError()
    {
        var rig = Build(_ => Transient);
        await rig.Worker.StartAsync(TestContext.Current.CancellationToken);

        rig.Queue.TryEnqueue(Messages.To());
        await Pump(rig, () => rig.Logger.Entries.Any(e => e.Level == LogLevel.Error));
        rig.Time.Advance(TimeSpan.FromMinutes(1));
        await Task.Delay(50, TestContext.Current.CancellationToken);
        await rig.Worker.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(4, rig.Email.Attempts);
        Assert.Contains(
            rig.Logger.Entries,
            e => e.Level == LogLevel.Error && e.Message.Contains("email_verification")
        );
    }

    [Fact]
    public async Task Worker_KeepsProcessing_AfterAMessageFailsFinally()
    {
        var rig = Build(attempt => attempt == 1 ? Permanent : null);
        await rig.Worker.StartAsync(TestContext.Current.CancellationToken);

        rig.Queue.TryEnqueue(Messages.To("first@echo.test"));
        rig.Queue.TryEnqueue(Messages.To("second@echo.test"));
        await WaitFor(() => rig.Email.Attempts == 2);
        await rig.Worker.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, rig.Email.Attempts);
    }

    [Fact]
    public async Task Stop_WhileWaitingToRetry_EndsWithoutThrowing()
    {
        var rig = Build(_ => Transient);
        await rig.Worker.StartAsync(TestContext.Current.CancellationToken);

        rig.Queue.TryEnqueue(Messages.To());
        await WaitFor(() => rig.Email.Attempts == 1);

        var stop = rig.Worker.StopAsync(TestContext.Current.CancellationToken);

        await stop.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal(1, rig.Email.Attempts);
    }
}
