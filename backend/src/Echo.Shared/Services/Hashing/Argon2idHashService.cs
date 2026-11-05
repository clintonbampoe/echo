namespace Echo.Shared.Services.Hashing;

using System.Text;

// Argon2id with the OWASP minimum of 19 MiB and two passes, about 30 ms against bcrypt's 160-200 ms. The settings
// are stored in each hash, so they can change later without invalidating the hashes already stored.
public class Argon2idHashService(HashGate gate) : IPasswordHasher
{
    private const int _passes = 2;
    private const int _memoryMiB = 19;

    // Computed on the calling thread: a request already runs on a threadpool thread, so Task.Run would only add a hop.
    // When a permit is free the returned task is already finished. The gate lets only a few hashes run at once, which
    // also keeps memory to a few times 19 MiB; the rest wait without holding a thread (see HashGate).
    public async Task<string> HashAsync(string input, CancellationToken ct = default)
    {
        using var permit = await gate.EnterAsync(ct);
        var hash = Libsodium.Hash(Encoding.UTF8.GetBytes(input), _passes, _memoryMiB);
        return Encoding.ASCII.GetString(hash, 0, Array.IndexOf(hash, (byte)0));
    }

    // A stored value that is not an Argon2id hash (a row from before passwords were hashed, or a bcrypt hash) matches
    // nothing.
    public async Task<bool> VerifyAsync(string input, string hash, CancellationToken ct = default)
    {
        using var permit = await gate.EnterAsync(ct);

        var stored = new byte[Libsodium.HashBytes];
        if (Encoding.UTF8.GetByteCount(hash) >= stored.Length)
            return false;
        Encoding.UTF8.GetBytes(hash, stored);

        return Libsodium.Verify(stored, Encoding.UTF8.GetBytes(input));
    }
}
