using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace Echo.Shared.Services.Email;

public sealed class EmailQueue : IEmailQueue
{
    public const int Capacity = 1000;

    private readonly Channel<EmailMessage> _channel = Channel.CreateBounded<EmailMessage>(
        new BoundedChannelOptions(Capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
        }
    );

    private readonly ILogger<EmailQueue> _logger;

    public EmailQueue(ILogger<EmailQueue> logger) => _logger = logger;

    public ChannelReader<EmailMessage> Reader => _channel.Reader;

    public bool TryEnqueue(EmailMessage message)
    {
        if (_channel.Writer.TryWrite(message))
            return true;

        EmailLog.QueueFull(_logger, message.Kind, message.UserId, message.CongregationId);
        return false;
    }
}
