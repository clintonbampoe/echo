namespace Echo.Shared.Services.Email;

public interface IEmailQueue
{
    /// <summary>
    /// Hands the message to the background sender and returns at once. False means the queue was
    /// full and the message was not accepted.
    /// </summary>
    bool TryEnqueue(EmailMessage message);
}
