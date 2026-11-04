namespace Echo.Shared.Services.Hashing;

using System.Runtime.InteropServices;

// The libsodium call that Sodium.Core's own wrapper does not allow: it refuses fewer than three passes, and the OWASP
// setting we use is two passes of 19 MiB. The native library ships inside the Sodium.Core package.
internal static class Libsodium
{
    public const int HashBytes = 128; // crypto_pwhash_STRBYTES: the encoded hash, with its terminating zero
    private const int _argon2id13 = 2; // crypto_pwhash_ALG_ARGON2ID13

    static Libsodium() => Sodium.SodiumCore.Init();

    [DllImport("libsodium")]
    private static extern int crypto_pwhash_str_alg(
        [Out] byte[] hash,
        [In] byte[] password,
        ulong passwordLength,
        ulong passes,
        nuint memoryBytes,
        int algorithm
    );

    [DllImport("libsodium")]
    private static extern int crypto_pwhash_str_verify(
        [In] byte[] hash,
        [In] byte[] password,
        ulong passwordLength
    );

    public static byte[] Hash(byte[] password, int passes, int memoryMiB)
    {
        var hash = new byte[HashBytes];
        var result = crypto_pwhash_str_alg(
            hash,
            password,
            (ulong)password.Length,
            (ulong)passes,
            (nuint)memoryMiB * 1024 * 1024,
            _argon2id13
        );

        return result == 0
            ? hash
            : throw new InvalidOperationException(
                "libsodium could not hash the password (out of memory?)"
            );
    }

    /// <param name="hash">The encoded hash, zero terminated, in a buffer of <see cref="HashBytes"/>.</param>
    public static bool Verify(byte[] hash, byte[] password) =>
        crypto_pwhash_str_verify(hash, password, (ulong)password.Length) == 0;
}
