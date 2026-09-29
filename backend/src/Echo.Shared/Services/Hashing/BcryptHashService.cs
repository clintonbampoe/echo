namespace Echo.Shared.Services.Hashing;

using BCrypt.Net;

public class BcryptHashService : IPasswordHasher
{
    private const int _costFactor = 10;

    public async Task<string> HashAsync(string input)
    {
        return await Task.Run(() => BCrypt.HashPassword(input, _costFactor));
    }

    public async Task<bool> VerifyAsync(string input, string hash)
    {
        return await Task.Run(() => BCrypt.Verify(input, hash));
    }
}
