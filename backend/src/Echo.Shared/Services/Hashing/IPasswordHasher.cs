namespace Echo.Shared.Services.Hashing;

public interface IPasswordHasher
{
    /// <exception cref="HashingBusyException">Too many password checks are already waiting.</exception>
    Task<string> HashAsync(string password, CancellationToken ct = default);

    /// <exception cref="HashingBusyException">Too many password checks are already waiting.</exception>
    Task<bool> VerifyAsync(string password, string hash, CancellationToken ct = default);
}
