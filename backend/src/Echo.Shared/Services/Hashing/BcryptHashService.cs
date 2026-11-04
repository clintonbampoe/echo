namespace Echo.Shared.Services.Hashing;

using BCrypt.Net;

public class BcryptHashService(HashGate gate) : IPasswordHasher
{
    private const int _costFactor = 10;

    // Computed on the calling thread: a request already runs on a threadpool thread, so
    // Task.Run would only add a hop. When a permit is free the returned task is already finished. The gate lets only a
    // few hashes run at once; the rest wait without holding a thread (see HashGate).
    public async Task<string> HashAsync(string input, CancellationToken ct = default)
    {
        using var permit = await gate.EnterAsync(ct);
        return BCrypt.HashPassword(input, _costFactor);
    }

    // A stored value that is not a bcrypt hash (a row from before passwords were hashed) matches nothing.
    public async Task<bool> VerifyAsync(string input, string hash, CancellationToken ct = default)
    {
        using var permit = await gate.EnterAsync(ct);
        try
        {
            return BCrypt.Verify(input, hash);
        }
        catch (Exception ex) when (ex is SaltParseException or ArgumentException)
        {
            return false;
        }
    }
}
