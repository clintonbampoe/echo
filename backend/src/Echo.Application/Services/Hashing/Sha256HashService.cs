using System.Security.Cryptography;
using System.Text;

namespace Echo.Application.Services.Hashing;

public class Sha256HashService : ITokenHasher
{
    public Task<string> HashAsync(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = SHA256.HashData(bytes);
        var hash = Convert.ToBase64String(hashBytes);
        return Task.FromResult(hash);
    }

    public async Task<bool> VerifyAsync(string input, string hash)
    {
        var computedHash = await HashAsync(input);

        var computedBytes = Encoding.UTF8.GetBytes(computedHash);
        var expectedBytes = Encoding.UTF8.GetBytes(hash);

        // Constant-time comparison — prevents timing attacks on the comparison itself.
        // Lengths must match for FixedTimeEquals; mismatched length just means no match.
        if (computedBytes.Length != expectedBytes.Length)
            return false;

        var isEqual = CryptographicOperations.FixedTimeEquals(computedBytes, expectedBytes);
        return await Task.FromResult(isEqual);
    }
}
