namespace Echo.Shared.Services.Hashing;

/// <summary>
/// Too many password checks are already waiting: the caller should try again after <see cref="RetryAfter"/>.
/// </summary>
public sealed class HashingBusyException(TimeSpan retryAfter)
    : Exception("Too many password checks are waiting.")
{
    public TimeSpan RetryAfter { get; } = retryAfter;
}
