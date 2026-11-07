namespace Echo.Shared.Services.Hashing;

using System.Text;

public class Argon2idHashService(HashingMetrics metrics) : IPasswordHasher
{
    private const int _passes = 2;
    private const int _memoryMiB = 19;

    // Computed on the calling thread: a request already runs on a threadpool thread,
    // so Task.Run would only add a hop. When a permit is free the returned task is already finished.
    // The gate lets only a few hashes run at once, which also keeps memory to a few times 19 MiB;
    // the rest wait without holding a thread (see HashGate).
    public async Task<string> HashAsync(string input, CancellationToken ct = default)
    {
        using var permit = await metrics.EnterAsync(ct);
        var hash = Libsodium.Hash(Encoding.UTF8.GetBytes(input), _passes, _memoryMiB);
        return Encoding.ASCII.GetString(hash, 0, Array.IndexOf(hash, (byte)0));
    }

    public async Task<bool> VerifyAsync(string input, string hash, CancellationToken ct = default)
    {
        using var permit = await metrics.EnterAsync(ct);

        var stored = new byte[Libsodium.HashBytes];
        if (Encoding.UTF8.GetByteCount(hash) >= stored.Length)
            return false;
        Encoding.UTF8.GetBytes(hash, stored);

        return Libsodium.Verify(stored, Encoding.UTF8.GetBytes(input));
    }
}
