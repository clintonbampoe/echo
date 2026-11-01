using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Echo.Shared.Services.Email;

/// <summary>
/// Sends queued emails one at a time, retrying transient failures with back-off. A message that
/// cannot be delivered is logged and dropped; it never stops the worker.
/// </summary>
public sealed class EmailDeliveryWorker(
    EmailQueue queue,
    IServiceScopeFactory scopes,
    TimeProvider time,
    ILogger<EmailDeliveryWorker> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var message in queue.Reader.ReadAllAsync(stoppingToken))
                await Deliver(message, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    }

    private async Task Deliver(EmailMessage message, CancellationToken stoppingToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                using var scope = scopes.CreateScope();
                var email = scope.ServiceProvider.GetRequiredService<IEmailService>();
                await email.SendAsync(message.To, message.Content);
                EmailLog.Sent(logger, message.Kind, message.UserId, message.CongregationId);
                return;
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                var retriesLeft = attempt <= EmailRetryPolicy.Delays.Length;
                if (!retriesLeft || !EmailRetryPolicy.IsRetryable(ex))
                {
                    EmailLog.GaveUp(
                        logger,
                        ex,
                        message.Kind,
                        message.UserId,
                        message.CongregationId,
                        attempt
                    );
                    return;
                }

                var delay = EmailRetryPolicy.Delays[attempt - 1];
                EmailLog.Retrying(logger, ex, message.Kind, attempt, delay);
                await Task.Delay(delay, time, stoppingToken);
            }
        }
    }
}
