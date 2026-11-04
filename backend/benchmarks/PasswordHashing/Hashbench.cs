using System.Text;
using BenchmarkDotNet.Attributes;
using Konscious.Security.Cryptography;

namespace PasswordHashing;

[MemoryDiagnoser]
public class HashBench
{
    const string _pw = "correct horse battery staple";
    static readonly byte[] _salt = [.. Enumerable.Range(1, 16).Select(i => (byte)i)];

    [Benchmark(Baseline = true)]
    public string Bcrypt_Cost10() => BCrypt.Net.BCrypt.HashPassword(_pw, 10);

    [Benchmark]
    public string Bcrypt_Cost9() => BCrypt.Net.BCrypt.HashPassword(_pw, 9);

    [Benchmark]
    public byte[] Argon2id_19MiB_2p_Native() =>
        NativeArgon2.Hash(_pw, _salt, passes: 2, memoryMiB: 19);

    [Benchmark]
    public byte[] Argon2id_12MiB_3p_Native() =>
        NativeArgon2.Hash(_pw, _salt, passes: 3, memoryMiB: 12);

    [Benchmark]
    public byte[] Argon2id_46MiB_1p_Native() =>
        NativeArgon2.Hash(_pw, _salt, passes: 1, memoryMiB: 46);

    [Benchmark]
    public byte[] Argon2id_19MiB_2p_Managed()
    {
        var a = new Argon2id(Encoding.UTF8.GetBytes(_pw))
        {
            Salt = _salt,
            DegreeOfParallelism = 1,
            Iterations = 2,
            MemorySize = 19 * 1024,
        };

        return a.GetBytes(32);
    }
}
