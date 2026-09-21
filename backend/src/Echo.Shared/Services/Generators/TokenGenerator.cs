using System.Security.Cryptography;

namespace Echo.Shared.Services.Generators;

public class TokenGenerator : ITokenGenerator
{
    public string GenerateToken(int size)
    {
        if (size > 32)
            size = 32;

        var bytes = RandomNumberGenerator.GetBytes(size);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
