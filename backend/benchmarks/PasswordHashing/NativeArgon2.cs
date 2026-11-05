using System.Runtime.InteropServices;
using System.Text;

namespace PasswordHashing;

public static partial class NativeArgon2
{
    const int _argon2id13 = 2;

    [LibraryImport("libsodium")]
    public static partial int crypto_pwhash(
        [Out] byte[] output,
        ulong outputLength,
        [In] byte[] password,
        ulong passwordLength,
        [In] byte[] salt,
        ulong opsLimit,
        nuint memLimit,
        int alg
    );

    public static byte[] Hash(string password, byte[] salt, int passes, int memoryMiB)
    {
        Sodium.SodiumCore.Init();
        var pw = Encoding.UTF8.GetBytes(password);

        var output = new byte[32];
        var rc = crypto_pwhash(
            output,
            (ulong)output.Length,
            pw,
            (ulong)pw.Length,
            salt,
            (ulong)passes,
            (nuint)((long)memoryMiB * 1024 * 1024),
            _argon2id13
        );

        if (rc != 0)
            throw new InvalidOperationException(
                $"crypto_pwhash failed ({rc}), likely out of memory"
            );
        return output;
    }
}
